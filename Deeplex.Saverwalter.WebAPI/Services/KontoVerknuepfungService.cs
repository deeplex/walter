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
    }

    /// <summary>
    /// Verknüpfung eines Buchungskontos zu der Entität, der es zugeordnet ist
    /// (Vertrag, Wohnung, Umlage, Kontakt, Garage oder Garagenvertrag), samt der
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

    /// <summary>
    /// Buchungskonten haben keine Rückwärtsnavigation zu ihren Besitzern — die
    /// Zuordnung läuft über die Konto-Properties der Entitäten (z.B.
    /// Vertrag.MietBuchungskonto). Dieser Service löst sie rückwärts auf.
    /// </summary>
    public static class KontoVerknuepfungService
    {
        public static async Task<List<KontoVerknuepfungEntry>> ForKontenAsync(
            SaverwalterContext ctx, List<int> kontoIds)
        {
            var result = new List<KontoVerknuepfungEntry>();

            void Add(Buchungskonto? konto, string typ, string id, string text, KontoFunktion funktion)
            {
                if (konto != null && kontoIds.Contains(konto.BuchungskontoId))
                {
                    result.Add(new KontoVerknuepfungEntry
                    {
                        Typ = typ,
                        Id = id,
                        Text = text,
                        Funktion = funktion.Name,
                        Ausgleichbar = funktion.Ausgleichbar,
                        KontoId = konto.BuchungskontoId
                    });
                }
            }

            var vertraege = await ctx.Vertraege
                .Where(v =>
                    kontoIds.Contains(v.MietBuchungskonto.BuchungskontoId) ||
                    kontoIds.Contains(v.NkBuchungskonto.BuchungskontoId) ||
                    kontoIds.Contains(v.BkAbrechnungsKonto.BuchungskontoId) ||
                    kontoIds.Contains(v.ZahlungsKonto.BuchungskontoId) ||
                    kontoIds.Contains(v.MietminderungsKonto.BuchungskontoId))
                .ToListAsync();
            foreach (var v in vertraege)
            {
                var id = v.VertragId.ToString();
                var text = GetVertragName(v);
                Add(v.MietBuchungskonto, "Vertrag", id, text, KontoFunktion.Mietforderungen);
                Add(v.NkBuchungskonto, "Vertrag", id, text, KontoFunktion.NkVorauszahlungen);
                Add(v.BkAbrechnungsKonto, "Vertrag", id, text, KontoFunktion.BkAbrechnung);
                Add(v.ZahlungsKonto, "Vertrag", id, text, KontoFunktion.Zahlungseingaenge);
                Add(v.MietminderungsKonto, "Vertrag", id, text, KontoFunktion.Mietminderungen);
            }

            var wohnungen = await ctx.Wohnungen
                .Where(w =>
                    kontoIds.Contains(w.MietErtragskonto.BuchungskontoId) ||
                    kontoIds.Contains(w.AufwandsKonto.BuchungskontoId))
                .ToListAsync();
            foreach (var w in wohnungen)
            {
                var id = w.WohnungId.ToString();
                var text = $"{w.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {w.Bezeichnung}";
                Add(w.MietErtragskonto, "Wohnung", id, text, KontoFunktion.Mietertraege);
                Add(w.AufwandsKonto, "Wohnung", id, text, KontoFunktion.Aufwendungen);
            }

            var umlagen = await ctx.Umlagen
                .Where(u =>
                    kontoIds.Contains(u.NkVerrechnungsKonto.BuchungskontoId) ||
                    kontoIds.Contains(u.ZahlungsKonto.BuchungskontoId) ||
                    (u.NkSonderVerrechnungsKonto != null &&
                     kontoIds.Contains(u.NkSonderVerrechnungsKonto.BuchungskontoId)))
                .ToListAsync();
            foreach (var u in umlagen)
            {
                var id = u.UmlageId.ToString();
                var text = u.Typ.Bezeichnung;
                Add(u.NkVerrechnungsKonto, "Umlage", id, text, KontoFunktion.NkVerrechnung);
                Add(u.ZahlungsKonto, "Umlage", id, text, KontoFunktion.Zahlungseingaenge);
                Add(u.NkSonderVerrechnungsKonto, "Umlage", id, text, KontoFunktion.NkSonderVerrechnung);
            }

            var kontakte = await ctx.Kontakte
                .Where(k =>
                    k.VerbindlichkeitsKonto != null &&
                    kontoIds.Contains(k.VerbindlichkeitsKonto.BuchungskontoId))
                .ToListAsync();
            foreach (var k in kontakte)
            {
                Add(k.VerbindlichkeitsKonto, "Kontakt", k.KontaktId.ToString(),
                    k.Bezeichnung, KontoFunktion.Verbindlichkeiten);
            }

            var garagen = await ctx.Garagen
                .Where(g => kontoIds.Contains(g.Ertragskonto.BuchungskontoId))
                .ToListAsync();
            foreach (var g in garagen)
            {
                Add(g.Ertragskonto, "Garage", g.GarageId.ToString(), g.Kennung, KontoFunktion.Mietertraege);
            }

            var garageVertraege = await ctx.GarageVertraege
                .Where(gv =>
                    kontoIds.Contains(gv.MietBuchungskonto.BuchungskontoId) ||
                    kontoIds.Contains(gv.ZahlungsKonto.BuchungskontoId))
                .ToListAsync();
            foreach (var gv in garageVertraege)
            {
                var id = gv.GarageVertragId.ToString();
                var mieter = string.Join(", ", gv.Mieter.Select(m => m.Bezeichnung));
                var text = $"{gv.Garage.Kennung} | {mieter}";
                Add(gv.MietBuchungskonto, "GarageVertrag", id, text, KontoFunktion.Mietforderungen);
                Add(gv.ZahlungsKonto, "GarageVertrag", id, text, KontoFunktion.Zahlungseingaenge);
            }

            return result;
        }

        public static string GetVertragName(Vertrag vertrag)
        {
            var wohnung = $"{vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {vertrag.Wohnung.Bezeichnung}";
            var mieterText = string.Join(", ", vertrag.Mieter.Select(person => person.Bezeichnung));

            return $"{wohnung} - {mieterText}";
        }
    }
}
