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

using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.GarageVertragController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class GarageVertragControllerTests
    {
        private static readonly DateOnly Januar = new(2024, 1, 1);
        private static readonly DateOnly Februar = new(2024, 2, 1);

        private static async Task<GarageVertrag> GarageAsync(SaverwalterContext ctx)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var gv = new GarageVertrag
            {
                Garage = new Garage("G1")
                {
                    Besitzer = vertrag.Ansprechpartner!,
                    Ertragskonto = new Buchungskonto("4100", "Garagenmiete", BuchungskontoTyp.Ertrag),
                },
                MietBuchungskonto = new Buchungskonto("1100", "Garage", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto("1101", "Garage Zahlung", BuchungskontoTyp.Aktiv),
            };
            gv.Versionen.Add(new GarageVertragVersion(Januar, 50m) { GarageVertrag = gv });
            ctx.GarageVertraege.Add(gv);
            await ctx.SaveChangesAsync();
            return gv;
        }

        private static GarageVertragController Controller(SaverwalterContext ctx)
        {
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            return new GarageVertragController(
                A.Fake<ILogger<GarageVertragController>>(),
                new GarageVertragDbService(ctx, auth),
                ctx,
                auth,
                new MietkontoService(ctx),
                A.Fake<HttpClient>());
        }

        [Fact]
        public async Task ForderungsstatusZaehltDieZahlungFuerDenZugeordnetenMonat()
        {
            var ctx = TestUtils.GetContext();
            var gv = await GarageAsync(ctx);
            // Januarmiete, gezahlt im Februar.
            await new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 50m,
                Zahlungsdatum = new DateOnly(2024, 2, 3),
                GaragenEingaenge =
                {
                    new TransaktionBuchungsService.StandaloneGaragenmietInput
                    {
                        GarageVertragId = gv.GarageVertragId,
                        BetreffenderMonat = Januar,
                        Betrag = 50m,
                    }
                },
            });
            var controller = Controller(ctx);

            var jan = (await controller.GetForderungsstatus(gv.GarageVertragId, Januar)).Result
                .Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<GarageForderungsstatusEntry>().Subject;
            var feb = (await controller.GetForderungsstatus(gv.GarageVertragId, Februar)).Result
                .Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<GarageForderungsstatusEntry>().Subject;

            jan.SchonGezahlt.Should().Be(50m);
            jan.VerbleibendeForderung.Should().Be(0m);
            feb.SchonGezahlt.Should().Be(0m);
        }

        [Fact]
        public async Task MietkontoNichtGefunden()
        {
            var ctx = TestUtils.GetContext();

            var result = await Controller(ctx).GetMietkonto(4711);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task MietkontoDesGaragenvertrags()
        {
            var ctx = TestUtils.GetContext();
            var gv = await GarageAsync(ctx);

            var result = await Controller(ctx).GetMietkonto(gv.GarageVertragId);

            var konto = result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<MietkontoService.Mietkonto>().Subject;
            konto.Monate.Should().NotBeEmpty().And.AllSatisfy(m => m.Garagen.Should().ContainSingle());
        }
    }
}
