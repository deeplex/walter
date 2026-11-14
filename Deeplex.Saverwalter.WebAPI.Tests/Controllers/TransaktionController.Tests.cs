// Copyright (c) 2023-2024 Kai Lawrence
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Services.Buchungen.TransaktionBuchungsService;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class TransaktionControllerTests
    {
        private static TransaktionController Controller(SaverwalterContext ctx, IAuthorizationService auth)
        {
            var controller = new TransaktionController(
                A.Fake<ILogger<TransaktionController>>(),
                new TransaktionDbService(ctx, auth, new BuchungssatzSchutzService(ctx), TestServices.Rueckgaengig(ctx)),
                new TransaktionBuchungsService(ctx),
                new NkAnteilBuchungsService(ctx),
                ctx,
                auth,
                A.Fake<HttpClient>());
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            return controller;
        }

        private static IAuthorizationService Auth(Func<object, bool> erlaubt)
        {
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .ReturnsLazily((ClaimsPrincipal _, object resource, IEnumerable<IAuthorizationRequirement> _) =>
                    Task.FromResult(erlaubt(resource) ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
            return auth;
        }

        [Fact]
        public async Task ZahlungEinerErfasstenBetriebskostenrechnungPrueftDieUmlageDerRechnung()
        {
            var ctx = TestUtils.GetContext();
            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Wasser"),
                NkVerrechnungsKonto = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1200", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(umlage);
            await ctx.SaveChangesAsync();
            var rechnung = await new BetriebskostenrechnungBuchungsService(ctx)
                .BucheRechnungAsync(umlage, 100m, new DateOnly(2024, 11, 1), 2024, null);
            var gepruefte = new List<object>();

            var result = await Controller(ctx, Auth(r => { gepruefte.Add(r); return true; }))
                .BuchenVorschau(new TransaktionsInput
                {
                    Betrag = 100m,
                    Zahlungsdatum = new DateOnly(2024, 12, 1),
                    BetriebskostenEingaenge =
                    {
                        new BetriebskostenEingangInput { ExistingBuchungssatzId = rechnung.BuchungssatzId, Betrag = 100m }
                    },
                });

            result.Result.Should().BeOfType<OkObjectResult>();
            gepruefte.Should().ContainSingle().Which.Should().Be(umlage);
        }

        [Fact]
        public async Task ZahlungEinerRechnungFuerErhaltungsaufwandBrauchtRechteAnDerWohnung()
        {
            var ctx = TestUtils.GetContext();
            var wohnung = new Wohnung("EA-Wohnung")
            {
                MietErtragskonto = new Buchungskonto("4000", "Mieterträge", BuchungskontoTyp.Ertrag),
                AufwandsKonto = new Buchungskonto("4900", "Aufwand", BuchungskontoTyp.Aufwand),
            };
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh);
            ctx.Wohnungen.Add(wohnung);
            ctx.Kontakte.Add(aussteller);
            await ctx.SaveChangesAsync();
            var rechnung = await new ErhaltungsaufwendungBuchungsService(ctx).BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, 100m, new DateOnly(2024, 3, 1), "Reparatur", null);

            var result = await Controller(ctx, Auth(r => r is not Wohnung))
                .BuchenVorschau(new TransaktionsInput
                {
                    Betrag = 100m,
                    Zahlungsdatum = new DateOnly(2024, 4, 1),
                    Erhaltungsaufwendungen =
                    {
                        new ErhaltungsaufwendungsInput { ExistingBuchungssatzId = rechnung.BuchungssatzId, Betrag = 100m }
                    },
                });

            result.Result.Should().BeOfType<ForbidResult>();
        }
    }
}
