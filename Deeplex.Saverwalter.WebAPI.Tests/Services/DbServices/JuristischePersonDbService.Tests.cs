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
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class JuristischePersonDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new KontaktDbService(ctx, auth);
            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = await service.Get(user, entity.KontaktId);

            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<KontaktEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new KontaktDbService(ctx, auth);
            var entity = new Kontakt("TestFirma", Rechtsform.gbr);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = await service.Delete(user, entity.KontaktId);

            result.Should().BeOfType<OkResult>();
            ctx.Kontakte.Find(entity.KontaktId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new KontaktDbService(ctx, auth);
            var entity = new Kontakt("TestFirma", Rechtsform.gmbh);
            var entry = new KontaktEntry(entity, new());

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
            var service = new KontaktDbService(ctx, auth);
            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();
            var entry = new KontaktEntry(entity, new());

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
            var service = new KontaktDbService(ctx, auth);
            var entity = new Kontakt("TestFirma", Rechtsform.gmbh);

            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var entry = new KontaktEntry(entity, new());
            entry.Email = "testfirma@saverwalter.de";

            var result = await service.Put(user, entity.KontaktId, entry);

            result.Value.Should().NotBeNull();
            var updatedEntity = ctx.Kontakte.Find(entity.KontaktId);
            if (updatedEntity == null)
            {
                throw new Exception("JuristischePerson not found");
            }
            updatedEntity.Email.Should().Be("testfirma@saverwalter.de");
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new KontaktDbService(ctx, auth);
            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            var entry = new KontaktEntry(entity, new());
            entry.Email = "testfirma@saverwalter.de";

            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.KontaktId + 1132, entry);

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
