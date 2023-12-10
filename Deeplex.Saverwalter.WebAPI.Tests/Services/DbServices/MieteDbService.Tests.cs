using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MieteDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new MieteDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            ctx.Mieten.Add(entity);
            ctx.SaveChanges();

            var result = await service.Get(user, entity.MieteId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<MieteEntryBase>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new MieteDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            ctx.Mieten.Add(entity);
            ctx.SaveChanges();

            var result = await service.Delete(user, entity.MieteId);

            result.Should().BeOfType<OkResult>();
            ctx.Mieten.Find(entity.MieteId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new MieteDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            var entry = new MieteEntryBase(entity);

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
            var service = new MieteDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            ctx.Mieten.Add(entity);
            ctx.SaveChanges();
            var entry = new MieteEntryBase(entity);

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
            var service = new MieteDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };

            ctx.Mieten.Add(entity);
            ctx.SaveChanges();

            var entry = new MieteEntryBase(entity);
            entry.Betrag = 2000;

            var result = await service.Put(user, entity.MieteId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Mieten.Find(entity.MieteId);
            if (updatedEntity == null)
            {
                throw new Exception("Miete not found");
            }
            updatedEntity.Betrag.Should().Be(2000);
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new MieteDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            var entry = new MieteEntryBase(entity);
            entry.Betrag = 2000;

            ctx.Mieten.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.MieteId + 111, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
