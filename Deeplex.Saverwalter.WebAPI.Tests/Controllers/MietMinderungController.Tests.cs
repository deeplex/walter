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
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MietminderungControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<MietminderungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MietminderungDbService(ctx, auth);
            var controller = new MietminderungController(logger, dbService, A.Fake<HttpClient>());
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
            var logger = A.Fake<ILogger<MietminderungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MietminderungDbService(ctx, auth);
            var controller = new MietminderungController(logger, dbService, A.Fake<HttpClient>());
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            var entry = new MietminderungEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<MietminderungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MietminderungDbService(ctx, auth);
            var controller = new MietminderungController(logger, dbService, A.Fake<HttpClient>());
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            if (vertrag.Mietminderungen.First() == null)
            {
                throw new NullReferenceException("Mietminderung is null");
            }

            var result = await controller.Get(vertrag.Mietminderungen.First().MietminderungId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<MietminderungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MietminderungDbService(ctx, auth);
            var controller = new MietminderungController(logger, dbService, A.Fake<HttpClient>());
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var entry = new MietminderungEntry(entity, new());
            entry.Ende = new DateOnly(2021, 1, 31);

            var result = await controller.Put(entity.MietminderungId, entry);

            result.Value.Should().NotBeNull();
            vertrag.Mietminderungen.First().Ende.Should().Be(new DateOnly(2021, 1, 31));
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<MietminderungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MietminderungDbService(ctx, auth);
            var controller = new MietminderungController(logger, dbService, A.Fake<HttpClient>());
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var id = entity.MietminderungId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Mietminderungen.Find(id).Should().BeNull();

        }
    }
}
