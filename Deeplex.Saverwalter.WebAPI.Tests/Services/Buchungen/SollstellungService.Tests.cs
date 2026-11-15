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
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.SollstellungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class SollstellungServiceTests
    {
        private static readonly DateOnly Januar = new(2024, 1, 1);
        /// <summary>Stichtag: Januar bis März 2024 sind fällig.</summary>
        private static readonly DateOnly Heute = new(2024, 3, 20);

        /// <summary>Vertrag ab 01/2024 mit 500 Kaltmiete.</summary>
        private static async Task<Vertrag> VertragAsync(SaverwalterContext ctx)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var version = vertrag.Versionen.Single();
            version.Beginn = Januar;
            version.Nebenkostenvorauszahlung = 150m;
            await ctx.SaveChangesAsync();
            return vertrag;
        }

        private static async Task<GarageVertrag> GarageAsync(SaverwalterContext ctx, Vertrag vertrag, decimal miete, bool gebunden)
        {
            var gv = new GarageVertrag
            {
                Garage = new Garage("G1")
                {
                    Besitzer = vertrag.Ansprechpartner!,
                    Ertragskonto = new Buchungskonto("4100", "Garagenmiete", BuchungskontoTyp.Ertrag),
                },
                Vertrag = gebunden ? vertrag : null,
                MietBuchungskonto = new Buchungskonto("1100", "Garage", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto("1101", "Garage Zahlung", BuchungskontoTyp.Aktiv),
            };
            gv.Versionen.Add(new GarageVertragVersion(Januar, miete) { GarageVertrag = gv });
            ctx.GarageVertraege.Add(gv);
            await ctx.SaveChangesAsync();
            return gv;
        }

        private static List<Buchungszeile> Sollzeilen(Buchungskonto konto) =>
            konto.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).ToList();

        [Fact]
        public async Task LegtDieFehlendenMonateBisZumLaufendenMonatAn()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);

            var angelegt = await new SollstellungService(ctx).AnlegenAsync(2024, Heute);

            angelegt.Should().Be(3);
            var soll = Sollzeilen(vertrag.MietBuchungskonto);
            soll.Should().HaveCount(3);
            soll.Select(z => z.Betrag).Should().OnlyContain(b => b == 500m);
            soll.Select(z => z.Buchungssatz.Buchungsdatum.Month).Should().BeEquivalentTo([1, 2, 3]);
            vertrag.Wohnung.MietErtragskonto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag).Should().Be(1500m);
        }

        [Fact]
        public async Task IstWiederholbarOhneDoppelteSollstellungen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var service = new SollstellungService(ctx);

            await service.AnlegenAsync(2024, Heute);
            var zweiterLauf = await service.AnlegenAsync(2024, Heute);

            zweiterLauf.Should().Be(0);
            Sollzeilen(vertrag.MietBuchungskonto).Should().HaveCount(3);
        }

        [Fact]
        public async Task LegtNichtsFuerMonateNachDemVertragsendeOderFuerKuenftigeJahreAn()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            vertrag.Ende = new DateOnly(2024, 2, 15);
            await ctx.SaveChangesAsync();
            var service = new SollstellungService(ctx);

            (await service.AnlegenAsync(2024, Heute)).Should().Be(2);
            (await service.AnlegenAsync(2025, Heute)).Should().Be(0);
            Sollzeilen(vertrag.MietBuchungskonto).Should().HaveCount(2);
        }

        [Fact]
        public async Task LegtDieGaragenmieteAnGebundenWieAlleinstehend()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var gebunden = await GarageAsync(ctx, vertrag, 50m, gebunden: true);
            var alleinstehend = await GarageAsync(ctx, vertrag, 70m, gebunden: false);

            var angelegt = await new SollstellungService(ctx).AnlegenAsync(2024, Heute);

            angelegt.Should().Be(3 + 3 + 3);
            Sollzeilen(gebunden.MietBuchungskonto).Select(z => z.Betrag).Should().Equal(50m, 50m, 50m);
            Sollzeilen(alleinstehend.MietBuchungskonto).Select(z => z.Betrag).Should().Equal(70m, 70m, 70m);
        }

        [Fact]
        public async Task EineBeimBuchenEntstandeneSollstellungWirdNichtDoppeltAngelegt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            await new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 650m,
                Zahlungsdatum = new DateOnly(2024, 2, 5),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 2, 1),
                        Kaltmiete = 500m,
                        NkVorauszahlung = 150m,
                    }
                }
            });
            Sollzeilen(vertrag.MietBuchungskonto).Where(z => z.Buchungssatz.Transaktion == null).Should().HaveCount(1);

            var angelegt = await new SollstellungService(ctx).AnlegenAsync(2024, Heute);

            angelegt.Should().Be(2);
            Sollzeilen(vertrag.MietBuchungskonto).Where(z => z.Buchungssatz.Transaktion == null).Should().HaveCount(3);
        }

        [Fact]
        public async Task EndpunktLegtAnUndMeldetDieZahl()
        {
            var ctx = TestUtils.GetContext();
            await VertragAsync(ctx);
            var controller = new SollstellungController(new SollstellungService(ctx));

            var result = await controller.Post(new SollstellungRequest { Jahr = 2024 });

            var ergebnis = result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<SollstellungResult>().Subject;
            ergebnis.Jahr.Should().Be(2024);
            ergebnis.Angelegt.Should().Be(12); // 2024 ist vorbei: alle zwölf Monate
        }

        [Fact]
        public async Task EndpunktLehntEinKuenftigesJahrAb()
        {
            var ctx = TestUtils.GetContext();
            var controller = new SollstellungController(new SollstellungService(ctx));

            var result = await controller.Post(new SollstellungRequest { Jahr = DateTime.Today.Year + 1 });

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
