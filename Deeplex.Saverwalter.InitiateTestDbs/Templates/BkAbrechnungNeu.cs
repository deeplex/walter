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
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.NkGruppenAbrechnungsService;

namespace Deeplex.Saverwalter.InitiateTestDbs.Templates
{
    internal static class BkAbrechnungNeu
    {
        public static async Task BucheJahresabrechnungAsync(SaverwalterContext ctx, int jahr)
        {
            var umlagen = await ctx.Umlagen
                .AsSplitQuery()
                .Include(u => u.Typ)
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Haben
                                 && z.Buchungssatz.Buchungsjahr == jahr))
                    .ThenInclude(z => z.Buchungssatz)
                        .ThenInclude(s => s.Buchungszeilen)
                            .ThenInclude(z => z.Buchungskonto)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.AufwandsKonto)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.Versionen)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.NkBuchungskonto)
                            .ThenInclude(k => k.Buchungszeilen
                                .Where(z => z.Buchungssatz.Buchungsjahr == jahr))
                            .ThenInclude(z => z.Buchungssatz)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.MietBuchungskonto)
                            .ThenInclude(k => k.Buchungszeilen
                                .Where(z => z.Buchungssatz.Buchungsjahr == jahr))
                            .ThenInclude(z => z.Buchungssatz)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.BkAbrechnungsKonto)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.Abrechnungsresultate
                            .Where(r => r.Buchungssatz.Buchungsjahr == jahr))
                        .ThenInclude(r => r.Buchungssatz)
                            .ThenInclude(s => s.Buchungszeilen)
                                .ThenInclude(z => z.Buchungskonto)
                .Include(u => u.Zaehler)
                    .ThenInclude(z => z.Staende)
                .Include(u => u.Zaehler)
                    .ThenInclude(z => z.Wohnung)
                .ToListAsync();

            var einheiten = ComputeEinheiten(umlagen, jahr);
            Console.WriteLine($"Buche BK-Jahresabrechnung {jahr}: {einheiten.Count} Einheiten...");

            int gebucht = 0;
            foreach (var einheit in einheiten)
            {
                foreach (var plan in einheit.Rechnungsplaene)
                {
                    var satz = plan.Buchungssatz;

                    var bereitsGebuchteKontoIds = satz.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Soll)
                        .Select(z => z.Buchungskonto.BuchungskontoId)
                        .ToHashSet();

                    foreach (var anteil in plan.Anteile)
                    {
                        var konto = anteil.Partei.Buchungskonto;
                        if (bereitsGebuchteKontoIds.Contains(konto.BuchungskontoId)) continue;
                        if (anteil.Betrag <= 0) continue;

                        var zeile = new Buchungszeile(SollHaben.Soll, anteil.Betrag)
                        {
                            Buchungssatz = satz,
                            Buchungskonto = konto
                        };
                        satz.Buchungszeilen.Add(zeile);
                        bereitsGebuchteKontoIds.Add(konto.BuchungskontoId);
                        gebucht++;
                    }
                }
            }

            await ctx.SaveChangesAsync();
            Console.WriteLine($"BK-Jahresabrechnung {jahr}: {gebucht} Anteile gebucht.");
        }
    }
}
