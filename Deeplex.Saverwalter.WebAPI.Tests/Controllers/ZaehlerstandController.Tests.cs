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

using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerstandControllerTests
    {
        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerstandController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerstandDbService(ctx, auth);
            var controller = new ZaehlerstandController(logger, dbService, A.Fake<HttpClient>());
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();

            var entity = new Zaehlerstand(new DateOnly(2021, 12, 31), 4000)
            {
                Zaehler = zaehler
            };
            var entry = new ZaehlerstandEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ZaehlerstandController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerstandDbService(ctx, auth);
            var controller = new ZaehlerstandController(logger, dbService, A.Fake<HttpClient>());

            var entity = vertrag.Wohnung.Zaehler.First().Staende.First();

            var result = await controller.Get(entity.ZaehlerstandId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerstandController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerstandDbService(ctx, auth);
            var controller = new ZaehlerstandController(logger, dbService, A.Fake<HttpClient>());
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = vertrag.Wohnung.Zaehler.First().Staende.First();

            var entry = new ZaehlerstandEntry(entity, new());
            entry.Stand = 5000;

            var result = await controller.Put(entity.ZaehlerstandId, entry);

            result.Value.Should().NotBeNull();
            entity.Stand.Should().Be(5000);
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ZaehlerstandController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerstandDbService(ctx, auth);
            var controller = new ZaehlerstandController(logger, dbService, A.Fake<HttpClient>());

            var entity = vertrag.Wohnung.Zaehler.First().Staende.First();
            var id = entity.ZaehlerstandId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Zaehlerstaende.Find(id).Should().BeNull();

        }
    }
}
