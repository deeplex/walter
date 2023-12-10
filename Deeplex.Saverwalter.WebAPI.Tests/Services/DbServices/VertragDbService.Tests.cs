using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var result = await service.Get(user, entity.VertragId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<VertragEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var result = await service.Delete(user, entity.VertragId);

            result.Should().BeOfType<OkResult>();
            ctx.Vertraege.Find(entity.VertragId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Vertrag()
            {
                Ansprechpartner = vertrag.Ansprechpartner,
                Wohnung = vertrag.Wohnung
            };
            var entry = new VertragEntry(entity);

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
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var entry = new VertragEntry(entity);

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
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var entry = new VertragEntry(entity);
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = await service.Put(user, entity.VertragId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Vertraege.Find(entity.VertragId);
            if (updatedEntity == null)
            {
                throw new Exception("Vertrag not found");
            }
            updatedEntity.Ende.Should().Be(new DateOnly(2021, 12, 31));
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new VertragDbService(ctx, auth);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var entry = new VertragEntry(entity);
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = await service.Put(user, entity.VertragId + 2220, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
