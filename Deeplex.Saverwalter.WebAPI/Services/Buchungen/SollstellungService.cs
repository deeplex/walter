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
    /// Legt die fehlenden Sollstellungen der Mieten und Garagenmieten an — der eigene Vorgang
    /// dafür. Lesende Seiten (Startseite, Mietkonto) legen nichts an: ein Monat ohne
    /// Sollstellung gilt dort als fällig mit der Miete der Vertragsversion. Beim Buchen einer
    /// Miete entsteht sie weiterhin bei Bedarf (<see cref="TransaktionBuchungsService"/>).
    ///
    /// Angelegt wird für ein Jahr bis einschließlich laufendem Monat und bis zum Vertragsende;
    /// vorhandene Sollstellungen bleiben unberührt, der Vorgang lässt sich also beliebig oft
    /// wiederholen. Er läuft unter der Sperre <see cref="Sollstellung.GesperrtAsync{T}"/>.
    /// </summary>
    public class SollstellungService(SaverwalterContext ctx)
    {
        /// <returns>Die Zahl der neu angelegten Sollstellungen.</returns>
        public Task<int> AnlegenAsync(int jahr, DateOnly heute) =>
            Sollstellung.GesperrtAsync(ctx, async () =>
            {
                var angelegt = await LegeMietenAnAsync(jahr, heute) + await LegeGaragenmietenAnAsync(jahr, heute);
                if (angelegt > 0) await ctx.SaveChangesAsync();
                return angelegt;
            });

        private async Task<int> LegeMietenAnAsync(int jahr, DateOnly heute)
        {
            var jahresende = new DateOnly(jahr, 12, 31);
            var jahresanfang = new DateOnly(jahr, 1, 1);

            var vertraege = await ctx.Vertraege
                .AsSplitQuery()
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.MietErtragskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz)
                            .ThenInclude(s => s.Buchungszeilen).ThenInclude(g => g.Buchungskonto)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.Transaktion)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.StornoVon)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.StornoNach)
                .Where(v => v.Versionen.Any(ver => ver.Beginn <= jahresende)
                         && (v.Ende == null || v.Ende >= jahresanfang))
                .ToListAsync();

            var angelegt = 0;
            foreach (var vertrag in vertraege)
            {
                var nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;
                foreach (var monat in Sollstellung.Monate(jahr, heute, vertrag.Beginn(), vertrag.Ende))
                {
                    var version = vertrag.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn);
                    if (version == null) continue;
                    if (vertrag.MietBuchungskonto.Buchungszeilen.Any(z => Sollstellung.IstImMonat(z, monat, nkKontoId)))
                        continue;

                    ctx.Buchungssaetze.Add(Sollstellung.ErstelleMiete(vertrag, monat, version.Grundmiete));
                    angelegt++;
                }
            }
            return angelegt;
        }

        private async Task<int> LegeGaragenmietenAnAsync(int jahr, DateOnly heute)
        {
            var jahresende = new DateOnly(jahr, 12, 31);
            var jahresanfang = new DateOnly(jahr, 1, 1);

            var garageVertraege = await ctx.GarageVertraege
                .AsSplitQuery()
                .Include(gv => gv.Versionen)
                .Include(gv => gv.Garage).ThenInclude(g => g.Ertragskonto)
                .Include(gv => gv.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.Transaktion)
                .Include(gv => gv.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.StornoVon)
                .Include(gv => gv.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.StornoNach)
                .Where(gv => gv.Versionen.Any(v => v.Beginn <= jahresende)
                          && (gv.Ende == null || gv.Ende >= jahresanfang))
                .ToListAsync();

            var angelegt = 0;
            foreach (var gv in garageVertraege)
            {
                foreach (var monat in Sollstellung.Monate(jahr, heute, gv.Beginn(), gv.Ende))
                {
                    var version = gv.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn);
                    if (version == null) continue;
                    if (gv.MietBuchungskonto.Buchungszeilen.Any(z => Sollstellung.IstImMonat(z, monat)))
                        continue;

                    ctx.Buchungssaetze.Add(Sollstellung.ErstelleGarage(gv, monat, version.GaragenMiete));
                    angelegt++;
                }
            }
            return angelegt;
        }
    }
}
