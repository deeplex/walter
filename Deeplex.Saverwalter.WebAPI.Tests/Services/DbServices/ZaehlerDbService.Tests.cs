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
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerDbServiceTests
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
            var service = new ZaehlerDbService(ctx, auth);

            var result = await service.Get(user, vertrag.Wohnung.Zaehler.First().ZaehlerId);

            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<ZaehlerEntry>();
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
            var service = new ZaehlerDbService(ctx, auth);

            var id = vertrag.Wohnung.Zaehler.First().ZaehlerId;
            var result = await service.Delete(user, id);

            result.Should().BeOfType<OkResult>();
            ctx.ZaehlerSet.Find(id).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ZaehlerDbService(ctx, auth);
            var entity = new Zaehler("Test", Zaehlertyp.Strom)
            {
                Wohnung = TestUtils.GetVertragForAbrechnung(ctx).Wohnung
            };
            var entry = new ZaehlerEntry(entity, new());

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
            var service = new ZaehlerDbService(ctx, auth);
            var entity = new Zaehler("Test", Zaehlertyp.Strom);

            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();

            var entry = new ZaehlerEntry(entity, new());

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
            var service = new ZaehlerDbService(ctx, auth);
            var entity = new Zaehler("Test", Zaehlertyp.Strom);
            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();
            var entry = new ZaehlerEntry(entity, new());
            entry.Kennnummer = "Neue Kennnummer";

            var result = await service.Put(user, entity.ZaehlerId, entry);

            result.Value.Should().NotBeNull();
            var updatedEntity = ctx.ZaehlerSet.Find(entity.ZaehlerId);
            if (updatedEntity == null)
            {
                throw new Exception("Zaehler not found");
            }
            updatedEntity.Kennnummer.Should().Be("Neue Kennnummer");
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ZaehlerDbService(ctx, auth);
            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung")
            };
            var entity = new Zaehler("Test", Zaehlertyp.Strom);
            var entry = new ZaehlerEntry(entity, new());
            entry.Kennnummer = "Neue Kennnummer";
            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.ZaehlerId + 1312, entry);

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
