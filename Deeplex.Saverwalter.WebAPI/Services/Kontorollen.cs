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

namespace Deeplex.Saverwalter.WebAPI.Services
{
    /// <summary>
    /// Funktion, die ein Buchungskonto für seine Besitzer-Entität erfüllt.
    /// Ausgleichbar bedeutet: Das Konto soll sich über die Zeit ausgleichen,
    /// ein Saldo ist also ein offener Posten (Forderung/Verbindlichkeit) — im
    /// Gegensatz zu Summenkonten (Erträge, Aufwendungen, Zahlungseingänge),
    /// die laufend anwachsen.
    /// </summary>
    public record KontoFunktion(string Name, bool Ausgleichbar)
    {
        public static readonly KontoFunktion Mietforderungen = new("Mietforderungen", true);
        public static readonly KontoFunktion NkVorauszahlungen = new("NK-Vorauszahlungen", false);
        public static readonly KontoFunktion BkAbrechnung = new("BK-Abrechnung", true);
        public static readonly KontoFunktion Zahlungseingaenge = new("Zahlungseingänge", false);
        public static readonly KontoFunktion Mietminderungen = new("Mietminderungen", false);
        // Nicht ausgleichbar: eine Kaution wird über Jahre gehalten und ist kein offener
        // Punkt des Jahresabschlusses.
        public static readonly KontoFunktion Kaution = new("Kaution", false);
        public static readonly KontoFunktion Mietertraege = new("Mieterträge", false);
        public static readonly KontoFunktion Aufwendungen = new("Aufwendungen", false);
        // Bewusst NICHT ausgleichbar: die NK-Verrechnung gleicht sich erst mit den
        // Dienstleisterzahlungen aus, die hier momentan nicht gebucht werden. Sie soll
        // den Jahresabschluss deshalb nicht als offener Punkt blockieren (nur gelistet).
        public static readonly KontoFunktion NkVerrechnung = new("NK-Verrechnung", false);
        // Wie NkVerrechnung nicht ausgleichbar: reines Verrechnungskonto für individuelle
        // Sonderforderungen, die über den Vertrag ausgeglichen werden, nicht hier.
        public static readonly KontoFunktion NkSonderVerrechnung = new("NK-Sonderverrechnung", false);
        public static readonly KontoFunktion Verbindlichkeiten = new("Verbindlichkeiten", true);
        public static readonly KontoFunktion Bankkonto = new("Bankkonto", false);
    }

    /// <summary>
    /// Verknüpfung eines Buchungskontos zu der Entität, der es zugeordnet ist
    /// (Vertrag, Wohnung, Umlage, Kontakt, Garage, Garagenvertrag oder Bankkonto), samt der
    /// Funktion, die das Konto dort erfüllt.
    /// </summary>
    public class KontoVerknuepfungEntry
    {
        public string Typ { get; set; } = "";
        public string Id { get; set; } = "";
        public string Text { get; set; } = "";
        public string Funktion { get; set; } = "";
        public bool Ausgleichbar { get; set; }
        public int KontoId { get; set; }
    }

    /// <summary>Rolle eines Buchungskontos bei seinem Besitzer (für die Zuordnung von Zahlungen).</summary>
    public enum KontoRolle
    {
        Geld, Kaltmiete, NkVorauszahlung, BkAbrechnung, Kaution, Mietminderung,
        Garagenmiete, Betriebskosten, Aufwand, Mietertrag, Garagenertrag, Verbindlichkeit
    }

    /// <summary>
    /// Ein Besitzer eines Kontos: Rolle (Zuordnung) und Funktion (Buchhaltung,
    /// Jahresabschluss), welches Objekt (<c>Typ</c>, <c>Id</c>, <c>Text</c>), ein kurzer
    /// Kontext für Klartexte und der Link zum passenden Abschnitt.
    /// </summary>
    public record KontoBesitz(
        KontoRolle Rolle, KontoFunktion Funktion, string Typ, string Id, string Text, string Kontext, string? Href)
    {
        public string Besitzer => $"{Typ} {Id}";
    }

    /// <summary>
    /// Besitzer der Buchungskonten — die eine Stelle, die sie auflöst, rückwärts aus den
    /// Konto-Feldern der Besitzer (Vertrag, Garagenvertrag, Umlage, Wohnung, Garage,
    /// Kontakt, Bankkonto); Buchungskonten haben keine Rückwärtsnavigation. Jedes Konto soll genau
    /// einen Besitzer haben; ein Konto mit mehreren ist <b>mehrdeutig</b> und bekommt keine
    /// Rolle — wer es auswertet, rät nicht. Ausnahme: hat ein Konto nur Geld-Rollen (ein
    /// Bankkonto hinter dem Zahlungskonto eines Vertrags), ist es eindeutig Geld.
    /// </summary>
    public static class Kontorollen
    {
        public class Ergebnis
        {
            public Dictionary<int, KontoBesitz> Eindeutig { get; } = [];
            public Dictionary<int, List<KontoBesitz>> Mehrdeutig { get; } = [];
        }

        private static string WohnungText(Wohnung w) =>
            w.Adresse is Adresse a ? $"{w.Bezeichnung}, {a.Strasse} {a.Hausnummer}" : w.Bezeichnung;

        private static string WohnungLang(Wohnung w) =>
            $"{w.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {w.Bezeichnung}";

        /// <summary>Anschrift, Wohnung und Mieter eines Vertrags.</summary>
        public static string VertragText(Vertrag vertrag) =>
            $"{WohnungLang(vertrag.Wohnung)} - {string.Join(", ", vertrag.Mieter.Select(m => m.Bezeichnung))}";

        /// <summary>
        /// Verknüpfungen der Konten zu ihren Besitzern; ein mehrdeutiges Konto hat mehrere.
        /// </summary>
        public static async Task<List<KontoVerknuepfungEntry>> VerknuepfungenAsync(
            SaverwalterContext ctx, IEnumerable<int> kontoIds)
        {
            var ids = kontoIds.ToHashSet();
            var rollen = await ErmittleAsync(ctx, ids);
            return rollen.Eindeutig.Select(e => (e.Key, Besitz: e.Value))
                .Concat(rollen.Mehrdeutig.SelectMany(e => e.Value.Select(b => (e.Key, Besitz: b))))
                .Select(e => new KontoVerknuepfungEntry
                {
                    KontoId = e.Key,
                    Typ = e.Besitz.Typ,
                    Id = e.Besitz.Id,
                    Text = e.Besitz.Text,
                    Funktion = e.Besitz.Funktion.Name,
                    Ausgleichbar = e.Besitz.Funktion.Ausgleichbar,
                })
                .ToList();
        }

        /// <param name="kontoIds">Nur diese Konten; <c>null</c>: alle.</param>
        public static async Task<Ergebnis> ErmittleAsync(SaverwalterContext ctx, IReadOnlySet<int>? kontoIds = null)
        {
            var alle = new Dictionary<int, List<KontoBesitz>>();
            void Add(Buchungskonto? konto, KontoRolle rolle, KontoFunktion funktion,
                string typ, int id, string text, string kontext, string? href)
            {
                if (konto == null || (kontoIds != null && !kontoIds.Contains(konto.BuchungskontoId))) return;
                if (!alle.TryGetValue(konto.BuchungskontoId, out var liste))
                    alle[konto.BuchungskontoId] = liste = [];
                liste.Add(new KontoBesitz(rolle, funktion, typ, id.ToString(), text, kontext, href));
            }
            bool Relevant(int id) => kontoIds == null || kontoIds.Contains(id);

            var vertraege = await ctx.Vertraege
                .Where(v => kontoIds == null
                    || kontoIds.Contains(v.MietBuchungskonto.BuchungskontoId)
                    || kontoIds.Contains(v.NkBuchungskonto.BuchungskontoId)
                    || kontoIds.Contains(v.BkAbrechnungsKonto.BuchungskontoId)
                    || kontoIds.Contains(v.KautionsKonto.BuchungskontoId)
                    || kontoIds.Contains(v.MietminderungsKonto.BuchungskontoId)
                    || kontoIds.Contains(v.ZahlungsKonto.BuchungskontoId))
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.Mieter)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.BkAbrechnungsKonto)
                .Include(v => v.KautionsKonto)
                .Include(v => v.MietminderungsKonto)
                .Include(v => v.ZahlungsKonto)
                .AsSplitQuery()
                .ToListAsync();
            foreach (var v in vertraege)
            {
                var text = VertragText(v);
                var kontext = WohnungText(v.Wohnung);
                var href = $"/vertraege/{v.VertragId}";
                void V(Buchungskonto k, KontoRolle r, KontoFunktion f, string h) => Add(k, r, f, "Vertrag", v.VertragId, text, kontext, h);
                V(v.MietBuchungskonto, KontoRolle.Kaltmiete, KontoFunktion.Mietforderungen, href + "#mietkonto");
                V(v.NkBuchungskonto, KontoRolle.NkVorauszahlung, KontoFunktion.NkVorauszahlungen, href + "#mietkonto");
                V(v.BkAbrechnungsKonto, KontoRolle.BkAbrechnung, KontoFunktion.BkAbrechnung, href + "#abrechnungen");
                V(v.KautionsKonto, KontoRolle.Kaution, KontoFunktion.Kaution, href + "#kaution");
                V(v.MietminderungsKonto, KontoRolle.Mietminderung, KontoFunktion.Mietminderungen, href);
                V(v.ZahlungsKonto, KontoRolle.Geld, KontoFunktion.Zahlungseingaenge, href);
            }

            var garageVertraege = await ctx.GarageVertraege
                .Where(gv => kontoIds == null
                    || kontoIds.Contains(gv.MietBuchungskonto.BuchungskontoId)
                    || kontoIds.Contains(gv.ZahlungsKonto.BuchungskontoId))
                .Include(gv => gv.Garage)
                .Include(gv => gv.Mieter)
                .Include(gv => gv.MietBuchungskonto)
                .Include(gv => gv.ZahlungsKonto)
                .ToListAsync();
            foreach (var gv in garageVertraege)
            {
                var text = $"{gv.Garage.Kennung} | {string.Join(", ", gv.Mieter.Select(m => m.Bezeichnung))}";
                var kontext = $"Garage {gv.Garage.Kennung}";
                var href = $"/garage-vertraege/{gv.GarageVertragId}";
                Add(gv.MietBuchungskonto, KontoRolle.Garagenmiete, KontoFunktion.Mietforderungen, "GarageVertrag", gv.GarageVertragId, text, kontext, href);
                Add(gv.ZahlungsKonto, KontoRolle.Geld, KontoFunktion.Zahlungseingaenge, "GarageVertrag", gv.GarageVertragId, text, kontext, href);
            }

            var umlagen = await ctx.Umlagen
                .Where(u => kontoIds == null
                    || kontoIds.Contains(u.NkVerrechnungsKonto.BuchungskontoId)
                    || kontoIds.Contains(u.ZahlungsKonto.BuchungskontoId)
                    || (u.NkSonderVerrechnungsKonto != null && kontoIds.Contains(u.NkSonderVerrechnungsKonto.BuchungskontoId)))
                .Include(u => u.Typ)
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.NkSonderVerrechnungsKonto)
                .Include(u => u.ZahlungsKonto)
                .ToListAsync();
            foreach (var u in umlagen)
            {
                var t = u.Typ.Bezeichnung;
                var href = $"/umlagen/{u.UmlageId}#rechnungen";
                Add(u.NkVerrechnungsKonto, KontoRolle.Betriebskosten, KontoFunktion.NkVerrechnung, "Umlage", u.UmlageId, t, t, href);
                Add(u.NkSonderVerrechnungsKonto, KontoRolle.Betriebskosten, KontoFunktion.NkSonderVerrechnung, "Umlage", u.UmlageId, t, t, href);
                Add(u.ZahlungsKonto, KontoRolle.Geld, KontoFunktion.Zahlungseingaenge, "Umlage", u.UmlageId, t, t, href);
            }

            var wohnungen = await ctx.Wohnungen
                .Where(w => kontoIds == null
                    || kontoIds.Contains(w.AufwandsKonto.BuchungskontoId)
                    || kontoIds.Contains(w.MietErtragskonto.BuchungskontoId))
                .Include(w => w.Adresse)
                .Include(w => w.AufwandsKonto)
                .Include(w => w.MietErtragskonto)
                .ToListAsync();
            foreach (var w in wohnungen)
            {
                Add(w.AufwandsKonto, KontoRolle.Aufwand, KontoFunktion.Aufwendungen, "Wohnung", w.WohnungId, WohnungLang(w), WohnungText(w), $"/wohnungen/{w.WohnungId}#erhaltungsaufwand");
                Add(w.MietErtragskonto, KontoRolle.Mietertrag, KontoFunktion.Mietertraege, "Wohnung", w.WohnungId, WohnungLang(w), WohnungText(w), $"/wohnungen/{w.WohnungId}");
            }

            var garagen = await ctx.Garagen
                .Where(g => kontoIds == null || kontoIds.Contains(g.Ertragskonto.BuchungskontoId))
                .Include(g => g.Ertragskonto)
                .ToListAsync();
            foreach (var g in garagen)
                Add(g.Ertragskonto, KontoRolle.Garagenertrag, KontoFunktion.Mietertraege, "Garage", g.GarageId, g.Kennung, $"Garage {g.Kennung}", $"/garagen/{g.GarageId}");

            var kontakte = await ctx.Kontakte
                .Where(k => k.VerbindlichkeitsKonto != null
                    && (kontoIds == null || kontoIds.Contains(k.VerbindlichkeitsKonto.BuchungskontoId)))
                .Include(k => k.VerbindlichkeitsKonto)
                .ToListAsync();
            foreach (var k in kontakte)
                Add(k.VerbindlichkeitsKonto, KontoRolle.Verbindlichkeit, KontoFunktion.Verbindlichkeiten, "Kontakt", k.KontaktId, k.Bezeichnung, k.Bezeichnung, $"/kontakte/{k.KontaktId}");

            var bankkonten = await ctx.Bankkontos
                .Where(b => kontoIds == null || kontoIds.Contains(b.BuchungsKonto.BuchungskontoId))
                .Include(b => b.BuchungsKonto)
                .ToListAsync();
            foreach (var b in bankkonten)
                Add(b.BuchungsKonto, KontoRolle.Geld, KontoFunktion.Bankkonto, "Bankkonto", b.BankkontoId, b.Iban ?? b.Bank ?? "", b.Iban ?? "", $"/bankkontos/{b.BankkontoId}");

            var ergebnis = new Ergebnis();
            foreach (var (id, liste) in alle.Where(e => Relevant(e.Key)))
            {
                if (liste.Count == 1 || liste.All(b => b.Rolle == KontoRolle.Geld))
                    ergebnis.Eindeutig[id] = liste[0];
                else
                    ergebnis.Mehrdeutig[id] = liste;
            }
            return ergebnis;
        }
    }
}
