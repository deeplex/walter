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
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ErhaltungsaufwednungDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Get(user, entity.ErhaltungsaufwendungId);

            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<ErhaltungsaufwendungEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Delete(user, entity.ErhaltungsaufwendungId);

            result.Should().BeOfType<OkResult>();
            ctx.Erhaltungsaufwendungen.Find(entity.ErhaltungsaufwendungId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            var entry = new ErhaltungsaufwendungEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();
            var entry = new ErhaltungsaufwendungEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };

            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var entry = new ErhaltungsaufwendungEntry(entity, new());
            entry.Betrag = 2000;

            var result = await service.Put(user, entity.ErhaltungsaufwendungId, entry);

            result.Value.Should().NotBeNull();
            var updatedEntity = ctx.Erhaltungsaufwendungen.Find(entity.ErhaltungsaufwendungId);
            if (updatedEntity == null)
            {
                throw new Exception("Erhaltungsaufwendung not found");
            }
            updatedEntity.Betrag.Should().Be(2000);
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var aussteller = new Kontakt("TestPerson", Rechtsform.ag);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            var entry = new ErhaltungsaufwendungEntry(entity, new());
            entry.Betrag = 2000;

            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.ErhaltungsaufwendungId + 11, entry);

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
