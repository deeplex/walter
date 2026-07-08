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

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Schutzstatus eines Buchungssatzes für Löschen und Stornieren.
    ///
    /// Regeln:
    /// - Sätze einer Betriebskostenabrechnung (Resultat-Satz) sind nur über die
    ///   Rückabwicklung des Abrechnungslaufs (ganze Gruppe) veränderbar.
    /// - Sätze, die in eine existierende Abrechnung eingeflossen sind
    ///   (NK-Vorauszahlungen/NK-Anteile des abgerechneten Jahres, Rechnungen
    ///   der beteiligten Umlagen), sind gesperrt, solange die Abrechnung existiert.
    /// - Löschen ist nur für "freie" Sätze erlaubt: zusätzlich keine
    ///   OPOS-Verknüpfung und kein Storno(-Partner) — sonst stornieren.
    /// </summary>
    public class BuchungssatzSchutz
    {
        public bool IstAbrechnungssatz { get; init; }
        public bool AbrechnungBetroffen { get; init; }
        public bool HatOposVerknuepfung { get; init; }
        public bool IstStorno { get; init; }
        public bool IstStorniert { get; init; }

        public bool KannStornieren =>
            !IstAbrechnungssatz && !AbrechnungBetroffen && !IstStorno && !IstStorniert;

        public bool KannLoeschen => KannStornieren && !HatOposVerknuepfung;

        public string? Sperrgrund =>
            IstAbrechnungssatz
                ? "Buchungssatz einer Betriebskostenabrechnung — Rückabwicklung nur über den Abrechnungslauf (gesamte Gruppe)."
                : AbrechnungBetroffen
                    ? "In eine Betriebskostenabrechnung eingeflossen — zuerst die Abrechnung des Jahres als Ganzes zurücknehmen."
                    : IstStorniert
                        ? "Bereits storniert."
                        : IstStorno
                            ? "Stornobuchung — kann nicht selbst storniert oder gelöscht werden."
                            : HatOposVerknuepfung
                                ? "Mit anderen Buchungssätzen ausgeglichen — bitte stornieren statt löschen."
                                : null;
    }

    public class BuchungssatzSchutzService(SaverwalterContext ctx)
    {
        public async Task<BuchungssatzSchutz> PruefeAsync(Buchungssatz satz)
        {
            var zeileIds = satz.Buchungszeilen.Select(z => z.BuchungszeileId).ToList();
            var kontoIds = satz.Buchungszeilen
                .Select(z => z.Buchungskonto.BuchungskontoId)
                .Distinct()
                .ToList();

            var istAbrechnungssatz = await ctx.Abrechnungsresultate
                .AnyAsync(r => r.Buchungssatz.BuchungssatzId == satz.BuchungssatzId);

            // Eingeflossen über das NK-Konto eines Vertrags (Vorauszahlungen und
            // NK-Anteile des abgerechneten Jahres) ...
            var vertragBetroffen = await ctx.Abrechnungsresultate
                .AnyAsync(r =>
                    r.Buchungssatz.Buchungsjahr == satz.Buchungsjahr &&
                    kontoIds.Contains(r.Vertrag.NkBuchungskonto.BuchungskontoId));

            // ... oder über das NK-Verrechnungskonto einer Umlage, deren Wohnungen
            // für das Jahr abgerechnet wurden (BK-Rechnungen, Umbuchungen).
            var umlageBetroffen = await ctx.Abrechnungsresultate
                .AnyAsync(r =>
                    r.Buchungssatz.Buchungsjahr == satz.Buchungsjahr &&
                    r.Vertrag.Wohnung.Umlagen.Any(u =>
                        kontoIds.Contains(u.NkVerrechnungsKonto.BuchungskontoId)));

            var hatOpos = await ctx.OffenePostenAusgleiche
                .AnyAsync(a =>
                    zeileIds.Contains(a.SollZeile.BuchungszeileId) ||
                    zeileIds.Contains(a.HabenZeile.BuchungszeileId));

            return new BuchungssatzSchutz
            {
                IstAbrechnungssatz = istAbrechnungssatz,
                AbrechnungBetroffen = !istAbrechnungssatz && (vertragBetroffen || umlageBetroffen),
                HatOposVerknuepfung = hatOpos,
                IstStorno = satz.StornoVon != null,
                IstStorniert = satz.StornoNach != null
            };
        }
    }
}
