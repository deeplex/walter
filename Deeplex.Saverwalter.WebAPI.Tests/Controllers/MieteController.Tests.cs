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
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MieteControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService, A.Fake<HttpClient>());
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
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService, A.Fake<HttpClient>());
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            var entry = new MieteEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService, A.Fake<HttpClient>());

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Miete is null");
            }

            var result = await controller.Get(vertrag.Mieten.First().MieteId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService, A.Fake<HttpClient>());

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Miete is null");
            }
            var entry = new MieteEntry(vertrag.Mieten.First(), new());
            entry.Betrag = 2000;

            var result = await controller.Put(vertrag.Mieten.First().MieteId, entry);

            result.Value.Should().NotBeNull();
            vertrag.Mieten.First().Betrag.Should().Be(2000);
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService, A.Fake<HttpClient>());

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = vertrag.Mieten.First().MieteId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Mieten.Find(id).Should().BeNull();

        }
    }
}
