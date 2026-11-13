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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class NkVorauszahlungTests
    {
        private static async Task ZahleAsync(SaverwalterContext ctx, Vertrag vertrag, DateOnly monat, decimal nk)
        {
            await new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 500m + nk,
                Zahlungsdatum = new DateOnly(monat.Year, monat.Month, 5),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = monat,
                        Kaltmiete = 500m,
                        NkVorauszahlung = nk,
                    }
                }
            });
        }

        private static async Task<decimal> SummeAsync(SaverwalterContext ctx, Vertrag vertrag, int jahr)
        {
            var zeilen = await ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == vertrag.NkBuchungskonto.BuchungskontoId)
                .ToListAsync();
            return NkVorauszahlung.Summe(vertrag, zeilen, jahr);
        }

        [Fact]
        public async Task StornierteZahlungZaehltNicht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            await ZahleAsync(ctx, vertrag, new DateOnly(2024, 1, 1), 150m);
            await ZahleAsync(ctx, vertrag, new DateOnly(2024, 2, 1), 150m);
            var falsch = await ctx.Buchungssaetze
                .Where(s => s.Transaktion != null
                    && s.Buchungszeilen.Any(z => z.Buchungskonto.BuchungskontoId == vertrag.NkBuchungskonto.BuchungskontoId))
                .OrderBy(s => s.Buchungsdatum)
                .LastAsync();
            await new StornoBuchungsService(ctx).StornierenAsync(falsch.BuchungssatzId, "falsch zugeordnet");

            (await SummeAsync(ctx, vertrag, 2024)).Should().Be(150m);
        }

        [Fact]
        public async Task MietKorrekturMindertDieVorauszahlung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var monat = new DateOnly(2024, 1, 1);
            await ZahleAsync(ctx, vertrag, monat, 150m);
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();
            await new MietKorrekturService(ctx).AnwendenAsync(
                vertrag.VertragId, new HashSet<(int, int)> { (2024, 1) }, "Grund");

            (await SummeAsync(ctx, vertrag, 2024)).Should().Be(50m);
        }

        [Fact]
        public async Task AnzeigeDesErgebnissesRechnetWieDieAbrechnung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var monat = new DateOnly(2024, 1, 1);
            await ZahleAsync(ctx, vertrag, monat, 150m);
            await ZahleAsync(ctx, vertrag, new DateOnly(2024, 2, 1), 150m);
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();
            await new MietKorrekturService(ctx).AnwendenAsync(
                vertrag.VertragId, new HashSet<(int, int)> { (2024, 1) }, "Grund");

            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            ctx.Abrechnungsresultate.Add(resultat);
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, 2024, 120m);
            await ctx.SaveChangesAsync();

            var entry = new AbrechnungsresultatEntry(resultat, new Permissions());
            entry.Vorauszahlung.Should().Be(200m, "300 gezahlt, davon 100 zur Kaltmiete umgebucht");
            entry.Saldo.Should().Be(120m);
            entry.Rechnungsbetrag.Should().Be(320m);
            (await SummeAsync(ctx, vertrag, 2024)).Should().Be(entry.Vorauszahlung,
                "das Abrechnungsergebnis selbst ist keine Vorauszahlung");
        }
    }
}
