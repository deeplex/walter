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
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlagetypControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService, A.Fake<HttpClient>());
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
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService, A.Fake<HttpClient>());

            var entity = new Umlagetyp("Hausstrom");
            var entry = new UmlagetypEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService, A.Fake<HttpClient>());

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Umlagetyp is null");
            }

            var result = await controller.Get(entity.UmlagetypId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService, A.Fake<HttpClient>());

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Umlagetyp is null");
            }
            var entry = new UmlagetypEntry(entity, new());
            entry.Bezeichnung = "Test";

            var result = await controller.Put(entity.UmlagetypId, entry);

            result.Value.Should().NotBeNull();
            entity.Bezeichnung.Should().Be("Test");
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService, A.Fake<HttpClient>());

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = entity.UmlagetypId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagetypen.Find(id).Should().BeNull();

        }
    }
}
