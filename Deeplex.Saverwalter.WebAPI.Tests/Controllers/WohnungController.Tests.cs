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
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class WohnungControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<WohnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new WohnungDbService(ctx, auth);
            var controller = new WohnungController(logger, dbService, A.Fake<HttpClient>());
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.Get();

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<WohnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new WohnungDbService(ctx, auth);
            var controller = new WohnungController(logger, dbService, A.Fake<HttpClient>());

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);
            ctx.SaveChanges();

            var entity = new Wohnung("Test", 100, 100, 100, 1)
            {
                Besitzer = besitzer
            };
            var entry = new WohnungEntry(entity, new());

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<WohnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new WohnungDbService(ctx, auth);
            var controller = new WohnungController(logger, dbService, A.Fake<HttpClient>());

            var entity = vertrag.Wohnung;

            var result = await controller.Get(entity.WohnungId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<WohnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new WohnungDbService(ctx, auth);
            var controller = new WohnungController(logger, dbService, A.Fake<HttpClient>());

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var entity = new Wohnung("Test", 100, 100, 100, 1)
            {
                Besitzer = besitzer
            };
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();
            var entry = new WohnungEntry(entity, new());
            entry.Wohnflaeche = 200;

            var result = await controller.Put(entity.WohnungId, entry);

            result.Value.Should().NotBeNull();
            entity.Wohnflaeche.Should().Be(200);
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<WohnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new WohnungDbService(ctx, auth);
            var controller = new WohnungController(logger, dbService, A.Fake<HttpClient>());

            var entity = vertrag.Wohnung;
            var id = entity.WohnungId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Wohnungen.Find(id).Should().BeNull();

        }
    }
}
