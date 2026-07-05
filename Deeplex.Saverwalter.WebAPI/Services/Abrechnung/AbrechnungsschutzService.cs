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

namespace Deeplex.Saverwalter.WebAPI.Services.Abrechnung
{
    /// <summary>
    /// Schützt abgerechnete Zeiträume: zeit-/mengenrelevante Stammdaten (Vertragsdauer,
    /// Zählerstände, Wohnungs-/Umlage-Versionen) dürfen nicht mehr geändert werden, wenn
    /// dadurch eine bereits GEBUCHTE Abrechnung verfälscht würde. Bewusst jahr-genau:
    /// Änderungen, die nur nicht-abgerechnete Jahre betreffen, bleiben erlaubt.
    ///
    /// „Abgerechnet" = es existiert ein (nicht storniertes) Abrechnungsresultat für das
    /// Jahr. Zum Ändern muss die Abrechnung dieses Jahres erst zurückgenommen/storniert werden.
    /// </summary>
    public static class AbrechnungsschutzService
    {
        /// <summary>Jahre mit gebuchtem (nicht storniertem) Abrechnungsresultat des Vertrags.</summary>
        public static async Task<HashSet<int>> AbgerechneteJahreVertrag(
            SaverwalterContext ctx, int vertragId)
        {
            var jahre = await ctx.Abrechnungsresultate
                .Where(r => r.Vertrag.VertragId == vertragId
                         && r.Buchungssatz.StornoNach == null)
                .Select(r => r.Buchungssatz.Buchungsjahr)
                .ToListAsync();
            return jahre.ToHashSet();
        }

        /// <summary>Jahre mit gebuchtem Resultat für irgendeinen Vertrag der Wohnungen.</summary>
        public static async Task<HashSet<int>> AbgerechneteJahreWohnungen(
            SaverwalterContext ctx, IEnumerable<int> wohnungIds)
        {
            var ids = wohnungIds.ToList();
            var jahre = await ctx.Abrechnungsresultate
                .Where(r => ids.Contains(r.Vertrag.Wohnung.WohnungId)
                         && r.Buchungssatz.StornoNach == null)
                .Select(r => r.Buchungssatz.Buchungsjahr)
                .ToListAsync();
            return jahre.ToHashSet();
        }

        /// <summary>Jahre mit gebuchtem Resultat für einen Vertrag einer Wohnung der Umlage.</summary>
        public static async Task<HashSet<int>> AbgerechneteJahreUmlage(
            SaverwalterContext ctx, int umlageId)
        {
            var wohnungIds = await ctx.Umlagen
                .Where(u => u.UmlageId == umlageId)
                .SelectMany(u => u.Wohnungen.Select(w => w.WohnungId))
                .ToListAsync();
            return await AbgerechneteJahreWohnungen(ctx, wohnungIds);
        }

        /// <summary>
        /// Wohnungen, die ein Zähler beeinflusst: die eigene Wohnung, oder — bei
        /// Allgemeinzählern (ohne Wohnung) — alle Wohnungen der Umlagen des Zählers.
        /// </summary>
        public static async Task<HashSet<int>> AbgerechneteJahreZaehler(
            SaverwalterContext ctx, int zaehlerId)
        {
            var wohnungId = await ctx.ZaehlerSet
                .Where(z => z.ZaehlerId == zaehlerId)
                .Select(z => (int?)z.Wohnung!.WohnungId)
                .FirstOrDefaultAsync();

            var wohnungIds = wohnungId.HasValue
                ? [wohnungId.Value]
                : await ctx.Umlagen
                    .Where(u => u.Zaehler.Any(z => z.ZaehlerId == zaehlerId))
                    .SelectMany(u => u.Wohnungen.Select(w => w.WohnungId))
                    .Distinct()
                    .ToListAsync();

            return await AbgerechneteJahreWohnungen(ctx, wohnungIds);
        }

        /// <summary>
        /// Jahre, die ein Zählerstand vom Datum <paramref name="datum"/> beeinflusst:
        /// das eigene Jahr; ein Dezember-Stand ist zusätzlich Anfangsstand des Folgejahres,
        /// ein Januar-Stand Endstand-Ersatz des Vorjahres.
        /// </summary>
        public static IEnumerable<int> StandBetroffeneJahre(DateOnly datum)
        {
            yield return datum.Year;
            if (datum.Month == 12) yield return datum.Year + 1;
            if (datum.Month == 1) yield return datum.Year - 1;
        }

        public static List<int> Schnittmenge(HashSet<int> abgerechneteJahre, IEnumerable<int> betroffene)
            => betroffene.Where(abgerechneteJahre.Contains).Distinct().OrderBy(j => j).ToList();

        /// <summary>
        /// Prüft, ob eine Änderung, die frühestens im Jahr <paramref name="betroffenAbJahr"/>
        /// wirkt, ein abgerechnetes Jahr trifft. Gibt die betroffenen Jahre zurück (leer = ok).
        /// Eine Datumsänderung wirkt ab dem frühesten berührten Jahr fort in die Zukunft.
        /// </summary>
        public static List<int> BetroffeneAbgerechneteJahre(
            HashSet<int> abgerechneteJahre, int betroffenAbJahr)
            => abgerechneteJahre.Where(j => j >= betroffenAbJahr).OrderBy(j => j).ToList();

        /// <summary>Frühestes berührtes Jahr, wenn ein Enddatum von alt auf neu wechselt.</summary>
        public static int FruehestesBetroffenesJahr(DateOnly? alt, DateOnly? neu)
            => Math.Min(alt?.Year ?? int.MaxValue, neu?.Year ?? int.MaxValue);

        public static string Sperrmeldung(IReadOnlyList<int> betroffeneJahre)
            => $"Für {string.Join(", ", betroffeneJahre)} ist bereits eine Abrechnung gebucht — "
             + "diese Änderung würde sie verfälschen. Bitte zuerst die Abrechnung des betroffenen "
             + "Jahres zurücknehmen bzw. stornieren.";

        // ── Bequeme Prüf-Methoden: liefern die Sperrmeldung oder null (= erlaubt) ──

        /// <summary>Änderung eines Vertrags, die ab <paramref name="abJahr"/> wirkt.</summary>
        public static async Task<string?> SperreVertrag(SaverwalterContext ctx, int vertragId, int abJahr)
        {
            var betroffen = BetroffeneAbgerechneteJahre(
                await AbgerechneteJahreVertrag(ctx, vertragId), abJahr);
            return betroffen.Count > 0 ? Sperrmeldung(betroffen) : null;
        }

        /// <summary>Änderung einer Wohnung, die ab <paramref name="abJahr"/> wirkt.</summary>
        public static async Task<string?> SperreWohnung(SaverwalterContext ctx, int wohnungId, int abJahr)
        {
            var betroffen = BetroffeneAbgerechneteJahre(
                await AbgerechneteJahreWohnungen(ctx, [wohnungId]), abJahr);
            return betroffen.Count > 0 ? Sperrmeldung(betroffen) : null;
        }

        /// <summary>Änderung einer Umlage, die ab <paramref name="abJahr"/> wirkt.</summary>
        public static async Task<string?> SperreUmlage(SaverwalterContext ctx, int umlageId, int abJahr)
        {
            var betroffen = BetroffeneAbgerechneteJahre(
                await AbgerechneteJahreUmlage(ctx, umlageId), abJahr);
            return betroffen.Count > 0 ? Sperrmeldung(betroffen) : null;
        }

        /// <summary>Änderung eines Zählers (z.B. Ende), die ab <paramref name="abJahr"/> wirkt.</summary>
        public static async Task<string?> SperreZaehlerAbJahr(SaverwalterContext ctx, int zaehlerId, int abJahr)
        {
            var betroffen = BetroffeneAbgerechneteJahre(
                await AbgerechneteJahreZaehler(ctx, zaehlerId), abJahr);
            return betroffen.Count > 0 ? Sperrmeldung(betroffen) : null;
        }

        /// <summary>Anlegen/Ändern/Löschen eines Zählerstands vom <paramref name="datum"/>.</summary>
        public static async Task<string?> SperreZaehlerstand(SaverwalterContext ctx, int zaehlerId, DateOnly datum)
        {
            var betroffen = Schnittmenge(
                await AbgerechneteJahreZaehler(ctx, zaehlerId), StandBetroffeneJahre(datum));
            return betroffen.Count > 0 ? Sperrmeldung(betroffen) : null;
        }
    }
}
