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
    /// Jedes Konto hat genau eine Rolle bei genau einem Besitzer (Vertrag, Garagenvertrag,
    /// Umlage, Wohnung, Kontakt, Bankkonto). Geld-Konten sind die Buchungskonten der
    /// Bankkonten und die Zahlungskonten von Vertrag, Garagenvertrag und Umlage.
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
        private enum Rolle
        {
            Geld, Kaltmiete, NkVorauszahlung, BkAbrechnung, Kaution, Mietminderung,
            Garagenmiete, Betriebskosten, Aufwand, Mietertrag, Garagenertrag, Verbindlichkeit
        }

        private record Bezug(Rolle Rolle, string Kontext, string? Href);

        private static readonly string[] Monate =
            ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"];

        private static string MonatText(DateOnly d) => $"{Monate[d.Month - 1]} {d.Year}";

        private static string WohnungText(Wohnung w) =>
            w.Adresse is Adresse a ? $"{w.Bezeichnung}, {a.Strasse} {a.Hausnummer}" : w.Bezeichnung;

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
            var bezuege = await BezuegeAsync(ctx, kontoIds);

            foreach (var satz in saetze)
            {
                var liste = ergebnis[satz.Transaktion!.TransaktionId];
                var storniert = satz.StornoNach != null;
                var eintraege = IstZahlung(satz, bezuege)
                    ? satz.Buchungszeilen
                        .Where(z => RolleVon(z, bezuege) != Rolle.Geld)
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

        private static Rolle? RolleVon(Buchungszeile z, Dictionary<int, Bezug> bezuege) =>
            bezuege.TryGetValue(z.Buchungskonto.BuchungskontoId, out var b) ? b.Rolle : null;

        private static bool IstZahlung(Buchungssatz satz, Dictionary<int, Bezug> bezuege) =>
            satz.Buchungszeilen.Any(z => RolleVon(z, bezuege) == Rolle.Geld);

        private static (string Text, string? Href, decimal Betrag) ZahlungsEintrag(
            Buchungszeile zeile, Buchungssatz satz, Dictionary<int, Bezug> bezuege)
        {
            if (!bezuege.TryGetValue(zeile.Buchungskonto.BuchungskontoId, out var bezug))
                return (KontoText(zeile.Buchungskonto), KontoHref(zeile.Buchungskonto), zeile.Betrag);

            var posten = zeile.ZugeordnetZu;
            string Mit(string art) => $"{art} – {bezug.Kontext}";
            var text = bezug.Rolle switch
            {
                Rolle.Kaltmiete or Rolle.Garagenmiete =>
                    Mit(posten is null
                        ? Art(bezug.Rolle)
                        : $"{Art(bezug.Rolle)} {MonatText(posten.Buchungssatz.Buchungsdatum)}"),
                Rolle.NkVorauszahlung => Mit($"NK-Vorauszahlung {satz.Buchungsjahr}"),
                Rolle.BkAbrechnung =>
                    Mit((zeile.SollHaben == SollHaben.Haben ? "Nachzahlung NK-Abrechnung" : "Erstattung NK-Abrechnung")
                        + (posten is null ? "" : $" {posten.Buchungssatz.Buchungsjahr}")),
                Rolle.Kaution => Mit(zeile.SollHaben == SollHaben.Haben ? "Kaution" : "Kaution Rückzahlung"),
                // Zahlung einer Rechnung: was die Rechnung war.
                Rolle.Betriebskosten or Rolle.Verbindlichkeit when posten is not null =>
                    null,
                Rolle.Betriebskosten => $"Betriebskosten {bezug.Kontext}",
                Rolle.Verbindlichkeit => Mit("Rechnung"),
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
            Buchungssatz satz, Dictionary<int, Bezug> bezuege)
        {
            Buchungszeile? Erste(Rolle rolle, SollHaben? seite = null) =>
                satz.Buchungszeilen.FirstOrDefault(z =>
                    RolleVon(z, bezuege) == rolle && (seite is null || z.SollHaben == seite));
            Bezug BezugVon(Buchungszeile z) => bezuege[z.Buchungskonto.BuchungskontoId];

            if ((Erste(Rolle.Betriebskosten, SollHaben.Haben) ?? Erste(Rolle.Betriebskosten)) is Buchungszeile bkZeile)
            {
                var bk = BezugVon(bkZeile);
                var betrag = satz.Buchungszeilen
                    .Where(z => z.Buchungskonto.BuchungskontoId == bkZeile.Buchungskonto.BuchungskontoId)
                    .Sum(z => z.SollHaben == bkZeile.SollHaben ? z.Betrag : -z.Betrag);
                return ($"Betriebskosten {bk.Kontext} {satz.Buchungsjahr}", bk.Href, betrag);
            }

            if (Erste(Rolle.Aufwand, SollHaben.Soll) is Buchungszeile aufwandZeile)
            {
                var aufwand = BezugVon(aufwandZeile);
                var betrag = satz.Buchungszeilen
                    .Where(z => RolleVon(z, bezuege) == Rolle.Aufwand && z.SollHaben == SollHaben.Soll)
                    .Sum(z => z.Betrag);
                return ($"Erhaltungsaufwand – {aufwand.Kontext}", aufwand.Href, betrag);
            }

            // Sonst: erste Zeile mit Besitzer außer dem Gläubiger, dann der Gläubiger, dann ein Konto.
            var zeile = satz.Buchungszeilen.FirstOrDefault(z => RolleVon(z, bezuege) is Rolle r && r != Rolle.Verbindlichkeit)
                ?? satz.Buchungszeilen.FirstOrDefault(z => RolleVon(z, bezuege) == Rolle.Verbindlichkeit)
                ?? satz.Buchungszeilen.First();
            if (!bezuege.TryGetValue(zeile.Buchungskonto.BuchungskontoId, out var b))
                return (KontoText(zeile.Buchungskonto), KontoHref(zeile.Buchungskonto), zeile.Betrag);
            var art = b.Rolle == Rolle.Verbindlichkeit ? "Rechnung" : Art(b.Rolle);
            return ($"{art} – {b.Kontext}", b.Href, zeile.Betrag);
        }

        private static string Art(Rolle rolle) => rolle switch
        {
            Rolle.Kaltmiete => "Kaltmiete",
            Rolle.NkVorauszahlung => "NK-Vorauszahlung",
            Rolle.BkAbrechnung => "NK-Abrechnung",
            Rolle.Kaution => "Kaution",
            Rolle.Mietminderung => "Mietminderung",
            Rolle.Garagenmiete => "Garagenmiete",
            Rolle.Betriebskosten => "Betriebskosten",
            Rolle.Aufwand => "Erhaltungsaufwand",
            Rolle.Mietertrag => "Mietertrag",
            Rolle.Garagenertrag => "Garagenertrag",
            Rolle.Verbindlichkeit => "Verbindlichkeit",
            _ => "Zahlung",
        };

        private static string KontoText(Buchungskonto k) => $"Buchungskonto {k.Kontonummer} {k.Bezeichnung}";
        private static string KontoHref(Buchungskonto k) => $"/buchungskonten/{k.BuchungskontoId}";

        private static async Task<Dictionary<int, Bezug>> BezuegeAsync(SaverwalterContext ctx, HashSet<int> kontoIds)
        {
            var bezuege = new Dictionary<int, Bezug>();
            void Add(Buchungskonto? konto, Rolle rolle, string kontext, string? href)
            {
                if (konto != null && kontoIds.Contains(konto.BuchungskontoId))
                    bezuege[konto.BuchungskontoId] = new Bezug(rolle, kontext, href);
            }

            var vertraege = await ctx.Vertraege
                .Where(v => kontoIds.Contains(v.MietBuchungskonto.BuchungskontoId)
                    || kontoIds.Contains(v.NkBuchungskonto.BuchungskontoId)
                    || kontoIds.Contains(v.BkAbrechnungsKonto.BuchungskontoId)
                    || kontoIds.Contains(v.KautionsKonto.BuchungskontoId)
                    || kontoIds.Contains(v.MietminderungsKonto.BuchungskontoId)
                    || kontoIds.Contains(v.ZahlungsKonto.BuchungskontoId))
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.BkAbrechnungsKonto)
                .Include(v => v.KautionsKonto)
                .Include(v => v.MietminderungsKonto)
                .Include(v => v.ZahlungsKonto)
                .ToListAsync();
            foreach (var v in vertraege)
            {
                var kontext = WohnungText(v.Wohnung);
                var href = $"/vertraege/{v.VertragId}";
                Add(v.MietBuchungskonto, Rolle.Kaltmiete, kontext, href + "#mietkonto");
                Add(v.NkBuchungskonto, Rolle.NkVorauszahlung, kontext, href + "#mietkonto");
                Add(v.BkAbrechnungsKonto, Rolle.BkAbrechnung, kontext, href + "#abrechnungen");
                Add(v.KautionsKonto, Rolle.Kaution, kontext, href + "#kaution");
                Add(v.MietminderungsKonto, Rolle.Mietminderung, kontext, href);
                Add(v.ZahlungsKonto, Rolle.Geld, kontext, href);
            }

            var garageVertraege = await ctx.GarageVertraege
                .Where(gv => kontoIds.Contains(gv.MietBuchungskonto.BuchungskontoId)
                    || kontoIds.Contains(gv.ZahlungsKonto.BuchungskontoId))
                .Include(gv => gv.Garage)
                .Include(gv => gv.MietBuchungskonto)
                .Include(gv => gv.ZahlungsKonto)
                .ToListAsync();
            foreach (var gv in garageVertraege)
            {
                var kontext = $"Garage {gv.Garage.Kennung}";
                var href = $"/garage-vertraege/{gv.GarageVertragId}";
                Add(gv.MietBuchungskonto, Rolle.Garagenmiete, kontext, href);
                Add(gv.ZahlungsKonto, Rolle.Geld, kontext, href);
            }

            var umlagen = await ctx.Umlagen
                .Where(u => kontoIds.Contains(u.NkVerrechnungsKonto.BuchungskontoId)
                    || kontoIds.Contains(u.ZahlungsKonto.BuchungskontoId)
                    || (u.NkSonderVerrechnungsKonto != null && kontoIds.Contains(u.NkSonderVerrechnungsKonto.BuchungskontoId)))
                .Include(u => u.Typ)
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.NkSonderVerrechnungsKonto)
                .Include(u => u.ZahlungsKonto)
                .ToListAsync();
            foreach (var u in umlagen)
            {
                var href = $"/umlagen/{u.UmlageId}#rechnungen";
                Add(u.NkVerrechnungsKonto, Rolle.Betriebskosten, u.Typ.Bezeichnung, href);
                Add(u.NkSonderVerrechnungsKonto, Rolle.Betriebskosten, u.Typ.Bezeichnung, href);
                Add(u.ZahlungsKonto, Rolle.Geld, u.Typ.Bezeichnung, href);
            }

            var wohnungen = await ctx.Wohnungen
                .Where(w => kontoIds.Contains(w.AufwandsKonto.BuchungskontoId)
                    || kontoIds.Contains(w.MietErtragskonto.BuchungskontoId))
                .Include(w => w.Adresse)
                .Include(w => w.AufwandsKonto)
                .Include(w => w.MietErtragskonto)
                .ToListAsync();
            foreach (var w in wohnungen)
            {
                Add(w.AufwandsKonto, Rolle.Aufwand, WohnungText(w), $"/wohnungen/{w.WohnungId}#erhaltungsaufwand");
                Add(w.MietErtragskonto, Rolle.Mietertrag, WohnungText(w), $"/wohnungen/{w.WohnungId}");
            }

            var garagen = await ctx.Garagen
                .Where(g => kontoIds.Contains(g.Ertragskonto.BuchungskontoId))
                .Include(g => g.Ertragskonto)
                .ToListAsync();
            foreach (var g in garagen)
                Add(g.Ertragskonto, Rolle.Garagenertrag, $"Garage {g.Kennung}", $"/garagen/{g.GarageId}");

            var kontakte = await ctx.Kontakte
                .Where(k => k.VerbindlichkeitsKonto != null && kontoIds.Contains(k.VerbindlichkeitsKonto.BuchungskontoId))
                .Include(k => k.VerbindlichkeitsKonto)
                .ToListAsync();
            foreach (var k in kontakte)
                Add(k.VerbindlichkeitsKonto, Rolle.Verbindlichkeit, k.Bezeichnung, $"/kontakte/{k.KontaktId}");

            var bankkonten = await ctx.Bankkontos
                .Where(b => kontoIds.Contains(b.BuchungsKonto.BuchungskontoId))
                .Include(b => b.BuchungsKonto)
                .ToListAsync();
            foreach (var b in bankkonten)
                Add(b.BuchungsKonto, Rolle.Geld, b.Iban ?? "", $"/bankkontos/{b.BankkontoId}");

            return bezuege;
        }
    }
}
