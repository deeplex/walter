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
    /// <summary>Rolle eines Buchungskontos bei seinem Besitzer.</summary>
    public enum KontoRolle
    {
        Geld, Kaltmiete, NkVorauszahlung, BkAbrechnung, Kaution, Mietminderung,
        Garagenmiete, Betriebskosten, Aufwand, Mietertrag, Garagenertrag, Verbindlichkeit
    }

    /// <summary>Ein Besitzer eines Kontos: welche Rolle, wessen Konto, wohin es führt.</summary>
    public record KontoBesitz(KontoRolle Rolle, string Besitzer, string Kontext, string? Href);

    /// <summary>
    /// Rollen der Buchungskonten, rückwärts aus den Konto-Feldern der Besitzer (Vertrag,
    /// Garagenvertrag, Umlage, Wohnung, Garage, Kontakt, Bankkonto). Jedes Konto soll genau
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

        /// <param name="kontoIds">Nur diese Konten; <c>null</c>: alle.</param>
        public static async Task<Ergebnis> ErmittleAsync(SaverwalterContext ctx, IReadOnlySet<int>? kontoIds = null)
        {
            var alle = new Dictionary<int, List<KontoBesitz>>();
            void Add(Buchungskonto? konto, KontoRolle rolle, string besitzer, string kontext, string? href)
            {
                if (konto == null || (kontoIds != null && !kontoIds.Contains(konto.BuchungskontoId))) return;
                if (!alle.TryGetValue(konto.BuchungskontoId, out var liste))
                    alle[konto.BuchungskontoId] = liste = [];
                liste.Add(new KontoBesitz(rolle, besitzer, kontext, href));
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
                var besitzer = $"Vertrag {v.VertragId}";
                var kontext = WohnungText(v.Wohnung);
                var href = $"/vertraege/{v.VertragId}";
                Add(v.MietBuchungskonto, KontoRolle.Kaltmiete, besitzer, kontext, href + "#mietkonto");
                Add(v.NkBuchungskonto, KontoRolle.NkVorauszahlung, besitzer, kontext, href + "#mietkonto");
                Add(v.BkAbrechnungsKonto, KontoRolle.BkAbrechnung, besitzer, kontext, href + "#abrechnungen");
                Add(v.KautionsKonto, KontoRolle.Kaution, besitzer, kontext, href + "#kaution");
                Add(v.MietminderungsKonto, KontoRolle.Mietminderung, besitzer, kontext, href);
                Add(v.ZahlungsKonto, KontoRolle.Geld, besitzer, kontext, href);
            }

            var garageVertraege = await ctx.GarageVertraege
                .Where(gv => kontoIds == null
                    || kontoIds.Contains(gv.MietBuchungskonto.BuchungskontoId)
                    || kontoIds.Contains(gv.ZahlungsKonto.BuchungskontoId))
                .Include(gv => gv.Garage)
                .Include(gv => gv.MietBuchungskonto)
                .Include(gv => gv.ZahlungsKonto)
                .ToListAsync();
            foreach (var gv in garageVertraege)
            {
                var besitzer = $"Garagenvertrag {gv.GarageVertragId}";
                var kontext = $"Garage {gv.Garage.Kennung}";
                var href = $"/garage-vertraege/{gv.GarageVertragId}";
                Add(gv.MietBuchungskonto, KontoRolle.Garagenmiete, besitzer, kontext, href);
                Add(gv.ZahlungsKonto, KontoRolle.Geld, besitzer, kontext, href);
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
                var besitzer = $"Umlage {u.UmlageId}";
                var href = $"/umlagen/{u.UmlageId}#rechnungen";
                Add(u.NkVerrechnungsKonto, KontoRolle.Betriebskosten, besitzer, u.Typ.Bezeichnung, href);
                Add(u.NkSonderVerrechnungsKonto, KontoRolle.Betriebskosten, besitzer, u.Typ.Bezeichnung, href);
                Add(u.ZahlungsKonto, KontoRolle.Geld, besitzer, u.Typ.Bezeichnung, href);
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
                var besitzer = $"Wohnung {w.WohnungId}";
                Add(w.AufwandsKonto, KontoRolle.Aufwand, besitzer, WohnungText(w), $"/wohnungen/{w.WohnungId}#erhaltungsaufwand");
                Add(w.MietErtragskonto, KontoRolle.Mietertrag, besitzer, WohnungText(w), $"/wohnungen/{w.WohnungId}");
            }

            var garagen = await ctx.Garagen
                .Where(g => kontoIds == null || kontoIds.Contains(g.Ertragskonto.BuchungskontoId))
                .Include(g => g.Ertragskonto)
                .ToListAsync();
            foreach (var g in garagen)
                Add(g.Ertragskonto, KontoRolle.Garagenertrag, $"Garage {g.GarageId}", $"Garage {g.Kennung}", $"/garagen/{g.GarageId}");

            var kontakte = await ctx.Kontakte
                .Where(k => k.VerbindlichkeitsKonto != null
                    && (kontoIds == null || kontoIds.Contains(k.VerbindlichkeitsKonto.BuchungskontoId)))
                .Include(k => k.VerbindlichkeitsKonto)
                .ToListAsync();
            foreach (var k in kontakte)
                Add(k.VerbindlichkeitsKonto, KontoRolle.Verbindlichkeit, $"Kontakt {k.KontaktId}", k.Bezeichnung, $"/kontakte/{k.KontaktId}");

            var bankkonten = await ctx.Bankkontos
                .Where(b => kontoIds == null || kontoIds.Contains(b.BuchungsKonto.BuchungskontoId))
                .Include(b => b.BuchungsKonto)
                .ToListAsync();
            foreach (var b in bankkonten)
                Add(b.BuchungsKonto, KontoRolle.Geld, $"Bankkonto {b.BankkontoId}", b.Iban ?? "", $"/bankkontos/{b.BankkontoId}");

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
