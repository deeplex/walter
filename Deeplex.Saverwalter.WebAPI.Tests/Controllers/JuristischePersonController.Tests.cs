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
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class JuristischePersonControllerTests
    {
        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new KontaktDbService(ctx, auth);
            var controller = new KontaktController(logger, dbService, A.Fake<HttpClient>());

            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            var entry = new KontaktEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new KontaktDbService(ctx, auth);
            var controller = new KontaktController(logger, dbService, A.Fake<HttpClient>());

            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = await controller.Get(entity.KontaktId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new KontaktDbService(ctx, auth);
            var controller = new KontaktController(logger, dbService, A.Fake<HttpClient>());

            var entity = new Kontakt("TestFirma", Rechtsform.gmbh);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var entry = new KontaktEntry(entity, new());
            entry.Email = "TestFirma@example.com";

            var result = await controller.Put(entity.KontaktId, entry);

            result.Value.Should().NotBeNull();
            entry.Email.Should().Be("TestFirma@example.com");
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new KontaktDbService(ctx, auth);
            var controller = new KontaktController(logger, dbService, A.Fake<HttpClient>());

            var entity = new Kontakt("TestFirma", Rechtsform.gbr);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();
            var id = entity.KontaktId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Kontakte.Find(id).Should().BeNull();

        }
    }
}
