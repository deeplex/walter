// Copyright (c) 2023-2026 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    /// <summary>Zuordnung einer Transaktion: wofür sie war, mit Link zum Fachobjekt.</summary>
    public class ZuordnungEintrag
    {
        public string Text { get; set; } = "";
        public string? Href { get; set; }
        public decimal Betrag { get; set; }
        /// <summary>Der Satz wurde storniert.</summary>
        public bool Storniert { get; set; }
    }

    /// <summary>
    /// Ermittelt die Zuordnung von Transaktionen allein aus der Struktur der Buchungen — nie
    /// aus Verwendungszweck oder Beschreibung.
    ///
    /// Die Rollen der Konten kommen aus <see cref="Kontorollen"/>. Geld-Konten sind die
    /// Buchungskonten der Bankkonten und die Zahlungskonten von Vertrag, Garagenvertrag und
    /// Umlage. Ein Konto mit mehreren Besitzern hat keine Rolle und erscheint als Konto.
    ///
    /// Jeder Satz einer Transaktion ist genau eins von beiden:
    /// - <b>Zahlung</b> (hat eine Geld-Zeile): jede andere Zeile ist ein Eintrag. Was sie
    ///   bezahlt hat, steht in <see cref="Buchungszeile.ZugeordnetZu"/> — beim Buchen gesetzt
    ///   und bei einem Storno unverändert. Monat bzw. Jahr kommen von dort (Sollstellung,
    ///   Abrechnung, Rechnung) oder aus der Zeile selbst, wo die Rolle sie festlegt
    ///   (NK-Vorauszahlung: Buchungsjahr). Fehlt die Zuordnung (Bestand ohne Ausgleich),
    ///   bleibt der Eintrag ohne Monat bzw. Jahr — es wird nichts geschätzt.
    /// - <b>Entstehung</b> (keine Geld-Zeile, z. B. eine Rechnung, an der die Transaktion als
    ///   Beleg hängt): genau ein Eintrag für den Posten. Eine Zeile auf einem
    ///   Verrechnungskonto einer Umlage macht den Satz zur Betriebskostenrechnung (ihre
    ///   übrigen Zeilen sind die Verteilung); sonst ist eine Soll-Zeile auf einem
    ///   Aufwandskonto Erhaltungsaufwand (das Verbindlichkeitskonto ist nur der Gläubiger).
    ///
    /// Zeilen auf Konten ohne bekannten Besitzer erscheinen mit dem Buchungskonto, damit
    /// nichts verloren geht. Eine Transaktion ohne Eintrag (nur Geld-Zeilen, z. B. eine
    /// Umbuchung zwischen Bankkonten) ist nicht zugeordnet.
    /// </summary>
    public static class Zuordnung
    {
        private static readonly string[] Monate =
            ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"];

        private static string MonatText(DateOnly d) => $"{Monate[d.Month - 1]} {d.Year}";

        /// <summary>Setzt <c>Zuordnung</c> an den Einträgen.</summary>
        public static async Task ZuordnenAsync(SaverwalterContext ctx, IEnumerable<TransaktionEntryBase> eintraege)
        {
            var liste = eintraege.ToList();
            var zuordnung = await ErmittleAsync(ctx, liste.Select(e => e.Id).ToList());
            foreach (var e in liste) e.Zuordnung = zuordnung[e.Id];
        }

        public static async Task<Dictionary<Guid, List<ZuordnungEintrag>>> ErmittleAsync(
            SaverwalterContext ctx, IReadOnlyCollection<Guid> transaktionIds)
        {
            var ergebnis = transaktionIds.Distinct().ToDictionary(id => id, _ => new List<ZuordnungEintrag>());
            if (ergebnis.Count == 0) return ergebnis;

            var ids = ergebnis.Keys.ToList();
            var saetze = await ctx.Buchungssaetze
                .AsSplitQuery()
                .Where(s => s.Transaktion != null && ids.Contains(s.Transaktion.TransaktionId) && s.StornoVon == null)
                .Include(s => s.Transaktion)
                .Include(s => s.StornoNach)
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.ZugeordnetZu).ThenInclude(p => p!.Buchungssatz)
                    .ThenInclude(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .OrderBy(s => s.Buchungsdatum)
                .ThenBy(s => s.Buchungsnummer)
                .ToListAsync();

            var kontoIds = saetze
                .SelectMany(s => s.Buchungszeilen)
                .SelectMany(z => (z.ZugeordnetZu?.Buchungssatz.Buchungszeilen ?? []).Append(z))
                .Select(z => z.Buchungskonto.BuchungskontoId)
                .ToHashSet();
            // Mehrdeutige Konten (mehrere Besitzer) bekommen keine Rolle: sie erscheinen als Konto.
            var bezuege = (await Kontorollen.ErmittleAsync(ctx, kontoIds)).Eindeutig;

            foreach (var satz in saetze)
            {
                var liste = ergebnis[satz.Transaktion!.TransaktionId];
                var storniert = satz.StornoNach != null;
                var eintraege = IstZahlung(satz, bezuege)
                    ? satz.Buchungszeilen
                        .Where(z => RolleVon(z, bezuege) != KontoRolle.Geld)
                        .Select(z => ZahlungsEintrag(z, satz, bezuege))
                    : [Entstehung(satz, bezuege)];
                foreach (var (text, href, betrag) in eintraege)
                {
                    var gleich = liste.FirstOrDefault(e => e.Text == text && e.Href == href && e.Storniert == storniert);
                    if (gleich != null) gleich.Betrag += betrag;
                    else liste.Add(new ZuordnungEintrag { Text = text, Href = href, Betrag = betrag, Storniert = storniert });
                }
            }

            return ergebnis;
        }

        private static KontoRolle? RolleVon(Buchungszeile z, Dictionary<int, KontoBesitz> bezuege) =>
            bezuege.TryGetValue(z.Buchungskonto.BuchungskontoId, out var b) ? b.Rolle : null;

        private static bool IstZahlung(Buchungssatz satz, Dictionary<int, KontoBesitz> bezuege) =>
            satz.Buchungszeilen.Any(z => RolleVon(z, bezuege) == KontoRolle.Geld);

        private static (string Text, string? Href, decimal Betrag) ZahlungsEintrag(
            Buchungszeile zeile, Buchungssatz satz, Dictionary<int, KontoBesitz> bezuege)
        {
            if (!bezuege.TryGetValue(zeile.Buchungskonto.BuchungskontoId, out var bezug))
                return (KontoText(zeile.Buchungskonto), KontoHref(zeile.Buchungskonto), zeile.Betrag);

            var posten = zeile.ZugeordnetZu;
            string Mit(string art) => $"{art} – {bezug.Kontext}";
            var text = bezug.Rolle switch
            {
                KontoRolle.Kaltmiete or KontoRolle.Garagenmiete =>
                    Mit(posten is null
                        ? Art(bezug.Rolle)
                        : $"{Art(bezug.Rolle)} {MonatText(posten.Buchungssatz.Buchungsdatum)}"),
                KontoRolle.NkVorauszahlung => Mit($"NK-Vorauszahlung {satz.Buchungsjahr}"),
                KontoRolle.BkAbrechnung =>
                    Mit((zeile.SollHaben == SollHaben.Haben ? "Nachzahlung NK-Abrechnung" : "Erstattung NK-Abrechnung")
                        + (posten is null ? "" : $" {posten.Buchungssatz.Buchungsjahr}")),
                KontoRolle.Kaution => Mit(zeile.SollHaben == SollHaben.Haben ? "Kaution" : "Kaution Rückzahlung"),
                // Zahlung einer Rechnung: was die Rechnung war.
                KontoRolle.Betriebskosten or KontoRolle.Verbindlichkeit when posten is not null =>
                    null,
                KontoRolle.Betriebskosten => $"Betriebskosten {bezug.Kontext}",
                KontoRolle.Verbindlichkeit => Mit("Rechnung"),
                _ => Mit(Art(bezug.Rolle)),
            };
            if (text is null)
            {
                var rechnung = Entstehung(posten!.Buchungssatz, bezuege);
                return (rechnung.Text, rechnung.Href, zeile.Betrag);
            }
            return (text, bezug.Href, zeile.Betrag);
        }

        /// <summary>Der eine Posten eines Satzes ohne Geld-Zeile (Rechnung).</summary>
        private static (string Text, string? Href, decimal Betrag) Entstehung(
            Buchungssatz satz, Dictionary<int, KontoBesitz> bezuege)
        {
            Buchungszeile? Erste(KontoRolle rolle, SollHaben? seite = null) =>
                satz.Buchungszeilen.FirstOrDefault(z =>
                    RolleVon(z, bezuege) == rolle && (seite is null || z.SollHaben == seite));
            KontoBesitz BezugVon(Buchungszeile z) => bezuege[z.Buchungskonto.BuchungskontoId];

            if ((Erste(KontoRolle.Betriebskosten, SollHaben.Haben) ?? Erste(KontoRolle.Betriebskosten)) is Buchungszeile bkZeile)
            {
                var bk = BezugVon(bkZeile);
                var betrag = satz.Buchungszeilen
                    .Where(z => z.Buchungskonto.BuchungskontoId == bkZeile.Buchungskonto.BuchungskontoId)
                    .Sum(z => z.SollHaben == bkZeile.SollHaben ? z.Betrag : -z.Betrag);
                return ($"Betriebskosten {bk.Kontext} {satz.Buchungsjahr}", bk.Href, betrag);
            }

            if (Erste(KontoRolle.Aufwand, SollHaben.Soll) is Buchungszeile aufwandZeile)
            {
                var aufwand = BezugVon(aufwandZeile);
                var betrag = satz.Buchungszeilen
                    .Where(z => RolleVon(z, bezuege) == KontoRolle.Aufwand && z.SollHaben == SollHaben.Soll)
                    .Sum(z => z.Betrag);
                return ($"Erhaltungsaufwand – {aufwand.Kontext}", aufwand.Href, betrag);
            }

            // Sonst: erste Zeile mit Besitzer außer dem Gläubiger, dann der Gläubiger, dann ein Konto.
            var zeile = satz.Buchungszeilen.FirstOrDefault(z => RolleVon(z, bezuege) is KontoRolle r && r != KontoRolle.Verbindlichkeit)
                ?? satz.Buchungszeilen.FirstOrDefault(z => RolleVon(z, bezuege) == KontoRolle.Verbindlichkeit)
                ?? satz.Buchungszeilen.First();
            if (!bezuege.TryGetValue(zeile.Buchungskonto.BuchungskontoId, out var b))
                return (KontoText(zeile.Buchungskonto), KontoHref(zeile.Buchungskonto), zeile.Betrag);
            var art = b.Rolle == KontoRolle.Verbindlichkeit ? "Rechnung" : Art(b.Rolle);
            return ($"{art} – {b.Kontext}", b.Href, zeile.Betrag);
        }

        private static string Art(KontoRolle rolle) => rolle switch
        {
            KontoRolle.Kaltmiete => "Kaltmiete",
            KontoRolle.NkVorauszahlung => "NK-Vorauszahlung",
            KontoRolle.BkAbrechnung => "NK-Abrechnung",
            KontoRolle.Kaution => "Kaution",
            KontoRolle.Mietminderung => "Mietminderung",
            KontoRolle.Garagenmiete => "Garagenmiete",
            KontoRolle.Betriebskosten => "Betriebskosten",
            KontoRolle.Aufwand => "Erhaltungsaufwand",
            KontoRolle.Mietertrag => "Mietertrag",
            KontoRolle.Garagenertrag => "Garagenertrag",
            KontoRolle.Verbindlichkeit => "Verbindlichkeit",
            _ => "Zahlung",
        };

        private static string KontoText(Buchungskonto k) => $"Buchungskonto {k.Kontonummer} {k.Bezeichnung}";
        private static string KontoHref(Buchungskonto k) => $"/buchungskonten/{k.BuchungskontoId}";
    }
}
