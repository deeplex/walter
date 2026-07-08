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
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var result = await service.Get(user, entity.VertragId);

            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<VertragEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var result = await service.Delete(user, entity.VertragId);

            result.Should().BeOfType<OkResult>();
            ctx.Vertraege.Find(entity.VertragId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Vertrag()
            {
                Ansprechpartner = vertrag.Ansprechpartner,
                Wohnung = vertrag.Wohnung,
                MietBuchungskonto = new Buchungskonto("1000", "Miete", BuchungskontoTyp.Aktiv),
                NkBuchungskonto = new Buchungskonto("1001", "NK-Vorauszahlung", BuchungskontoTyp.Passiv),
                BkAbrechnungsKonto = new Buchungskonto("1003", "BK-Abrechnung", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto("1004", "Zahlung", BuchungskontoTyp.Aktiv),
                MietminderungsKonto = new Buchungskonto("1005", "Mietminderung", BuchungskontoTyp.Aufwand),
            };
            var entry = new VertragEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var entry = new VertragEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var entry = new VertragEntry(entity, new());
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = await service.Put(user, entity.VertragId, entry);

            result.Value.Should().NotBeNull();
            var updatedEntity = ctx.Vertraege.Find(entity.VertragId);
            if (updatedEntity == null)
            {
                throw new Exception("Vertrag not found");
            }
            updatedEntity.Ende.Should().Be(new DateOnly(2021, 12, 31));
        }

        private static VertragDbService ServiceMitAuth(SaverwalterContext ctx, out ClaimsPrincipal user)
        {
            var u = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(u, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            user = u;
            return new VertragDbService(ctx, auth);
        }

        private static void BucheAbrechnung(SaverwalterContext ctx, Vertrag vertrag, int jahr)
        {
            var satz = new Buchungssatz(new DateOnly(jahr + 1, 1, 1), $"BK-Abrechnung {jahr}") { Buchungsjahr = jahr };
            ctx.Buchungssaetze.Add(satz);
            ctx.Abrechnungsresultate.Add(new Abrechnungsresultat { Vertrag = vertrag, Buchungssatz = satz });
            ctx.SaveChanges();
        }

        [Fact]
        public async Task Put_blockt_Ende_Aenderung_wenn_Jahr_abgerechnet()
        {
            var ctx = TestUtils.GetContext();
            var service = ServiceMitAuth(ctx, out var user);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            BucheAbrechnung(ctx, entity, 2021);

            var entry = new VertragEntry(entity, new());
            entry.Ende = new DateOnly(2021, 6, 30); // ändert das abgerechnete Jahr 2021

            var result = await service.Put(user, entity.VertragId, entry);

            result.Result.Should().BeOfType<ConflictObjectResult>();
            // Ende bleibt unverändert
            ctx.Vertraege.Find(entity.VertragId)!.Ende.Should().NotBe(new DateOnly(2021, 6, 30));
        }

        [Fact]
        public async Task Put_erlaubt_Ende_Aenderung_wenn_nur_nicht_abgerechnetes_Jahr_betroffen()
        {
            var ctx = TestUtils.GetContext();
            var service = ServiceMitAuth(ctx, out var user);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            BucheAbrechnung(ctx, entity, 2021);

            var entry = new VertragEntry(entity, new());
            entry.Ende = new DateOnly(2023, 6, 30); // betrifft nur 2023 (nicht abgerechnet)

            var result = await service.Put(user, entity.VertragId, entry);

            result.Value.Should().NotBeNull();
            ctx.Vertraege.Find(entity.VertragId)!.Ende.Should().Be(new DateOnly(2023, 6, 30));
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var entry = new VertragEntry(entity, new());
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = await service.Put(user, entity.VertragId + 2220, entry);

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
