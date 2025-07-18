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
using Deeplex.Saverwalter.WebAPI.Helper;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class WohnungDbServiceTests : IDisposable
    {
        public SaverwalterContext ctx;
        public WohnungDbServiceTests()
        {
            ctx = TestUtils.GetContext();
        }

        public void Dispose()
        {
            ctx.Dispose();
        }

        [Fact]
        public async Task GetTest()
        {
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);

            var result = await service.Get(user, vertrag.Wohnung.WohnungId);

            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<WohnungEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);

            var id = vertrag.Wohnung.WohnungId;
            var result = await service.Delete(user, id);

            result.Should().BeOfType<OkResult>();
            ctx.Wohnungen.Find(id).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);
            ctx.SaveChanges();

            var entity = new Wohnung("Test", 100, 100, 100, 1)
            {
                Besitzer = besitzer
            };

            var entry = new WohnungEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task PostFailedTest()
        {
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var entity = new Wohnung("Test", 100, 100, 100, 1)
            {
                Besitzer = besitzer
            };

            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();

            var entry = new WohnungEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);
            ctx.SaveChanges();

            var entity = new Wohnung("Test", 100, 100, 100, 1)
            {
                Besitzer = besitzer
            };
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();
            var entry = new WohnungEntry(entity, new());
            entry.Wohnflaeche = 200;

            var result = await service.Put(user, entity.WohnungId, entry);

            result.Value.Should().NotBeNull();
            var updatedEntity = ctx.Wohnungen.Find(entity.WohnungId);
            if (updatedEntity == null)
            {
                throw new Exception("Wohnung not found");
            }
            updatedEntity.Wohnflaeche.Should().Be(200);
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);
            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung")
            };

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var entity = new Wohnung("Test", 100, 100, 100, 1)
            {
                Besitzer = besitzer
            };
            var entry = new WohnungEntry(entity, new());
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.WohnungId + 11, entry);

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
