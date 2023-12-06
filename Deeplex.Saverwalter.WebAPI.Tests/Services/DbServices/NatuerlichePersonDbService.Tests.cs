using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class NatuerlichePersonDbServiceTests
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
            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = await service.Get(user, entity.KontaktId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<KontaktEntry>();
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
            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
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
            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
            var entry = new KontaktEntry(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<OkObjectResult>();
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
            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();
            var entry = new KontaktEntry(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<BadRequestResult>();
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
            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);

            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var entry = new KontaktEntry(entity);
            entry.Email = "TestPerson@saverwalter.de";

            var result = await service.Put(user, entity.KontaktId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Kontakte.Find(entity.KontaktId);
            if (updatedEntity == null)
            {
                throw new Exception("NatuerlichePerson not found");
            }
            updatedEntity.Email.Should().Be("TestPerson@saverwalter.de");
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
            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
            var entry = new KontaktEntry(entity);
            entry.Email = "TestPerson@saverwalter.de";

            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.KontaktId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
