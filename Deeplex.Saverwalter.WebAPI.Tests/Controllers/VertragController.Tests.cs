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
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService, ctx, auth, new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx)), A.Fake<HttpClient>());
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.Get(new PagedQuery());

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService, ctx, auth, new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx)), A.Fake<HttpClient>());

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var wohnung = new Wohnung("Test")
            {
                MietErtragskonto = new Buchungskonto("4000", "Mieterträge", BuchungskontoTyp.Ertrag),
                AufwandsKonto = new Buchungskonto("4900", "Aufwand", BuchungskontoTyp.Aufwand),
            };
            wohnung.Eigentuemer.Add(new WohnungEigentuemer(new DateOnly(2000, 1, 1)) { Wohnung = wohnung, Kontakt = besitzer });
            wohnung.Versionen.Add(new WohnungVersion(new DateOnly(2000, 1, 1), 100, 100, 100, 1) { Wohnung = wohnung });
            ctx.Wohnungen.Add(wohnung);
            ctx.SaveChanges();

            var entity = new Vertrag()
            {
                Ansprechpartner = besitzer,
                Wohnung = wohnung,
                MietBuchungskonto = new Buchungskonto("1000", "Miete", BuchungskontoTyp.Aktiv),
                NkBuchungskonto = new Buchungskonto("1001", "NK-Vorauszahlung", BuchungskontoTyp.Passiv),
                BkAbrechnungsKonto = new Buchungskonto("1003", "BK-Abrechnung", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto("1004", "Zahlung", BuchungskontoTyp.Aktiv),
                MietminderungsKonto = new Buchungskonto("1005", "Mietminderung", BuchungskontoTyp.Aufwand),
            };
            var entry = new VertragEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService, ctx, auth, new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx)), A.Fake<HttpClient>());

            var result = await controller.Get(entity.VertragId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService, ctx, auth, new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx)), A.Fake<HttpClient>());

            var entry = new VertragEntry(entity, new());
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = await controller.Put(entity.VertragId, entry);

            result.Value.Should().NotBeNull();
            entity.Ende.Should().Be(new DateOnly(2021, 12, 31));
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService, ctx, auth, new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx)), A.Fake<HttpClient>());

            var id = entity.VertragId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Vertraege.Find(id).Should().BeNull();

        }

        private static VertragController MietKorrekturController(SaverwalterContext ctx)
        {
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            return new VertragController(
                A.Fake<ILogger<VertragController>>(), new VertragDbService(ctx, auth), ctx, auth,
                new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx)), A.Fake<HttpClient>());
        }

        /// <summary>Bezahlter Monat 01/2024 (500 Kaltmiete, 150 NK-VZ) mit nachgetragener Erhöhung auf 600.</summary>
        private static async Task<Vertrag> VertragMitErhoehungAsync(SaverwalterContext ctx)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var monat = new DateOnly(2024, 1, 1);
            await new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 650m,
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = monat,
                        Kaltmiete = 500m,
                        NkVorauszahlung = 150m,
                    }
                }
            });
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();
            return vertrag;
        }

        [Fact]
        public async Task PostMietKorrektur_BuchtDieDifferenz()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragMitErhoehungAsync(ctx);

            var result = await MietKorrekturController(ctx).PostMietKorrektur(
                vertrag.VertragId, new MietKorrekturRequest { Jahr = 2024, Grund = "Mieterhöhung nachgetragen" });

            var jahr = result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<MietKorrekturJahrEntry>().Subject;
            jahr.Status.Should().Be("Stimmt");
            jahr.Korrigiert.Should().Be(100m);
            jahr.Korrekturen.Should().ContainSingle().Which.KannLoeschen.Should().BeTrue();
        }

        [Fact]
        public async Task PostMietKorrektur_OhneGrundIstBadRequest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragMitErhoehungAsync(ctx);

            var result = await MietKorrekturController(ctx).PostMietKorrektur(
                vertrag.VertragId, new MietKorrekturRequest { Jahr = 2024, Grund = " " });

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task PostMietKorrektur_ImAbgerechnetenJahrMeldetKonflikt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragMitErhoehungAsync(ctx);
            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, 2024, 0m);
            ctx.Abrechnungsresultate.Add(resultat);
            await ctx.SaveChangesAsync();
            var saetze = ctx.Buchungssaetze.Count();

            var result = await MietKorrekturController(ctx).PostMietKorrektur(
                vertrag.VertragId, new MietKorrekturRequest { Jahr = 2024, Grund = "Grund" });

            result.Result.Should().BeOfType<ConflictObjectResult>()
                .Which.Value.Should().BeOfType<string>().Which.Should().Contain("abgerechnet");
            ctx.Buchungssaetze.Count().Should().Be(saetze);
        }
    }
}
