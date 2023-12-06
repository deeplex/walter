using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MietminderungDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new MietminderungDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Get(user, entity.MietminderungId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<MietminderungEntryBase>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new MietminderungDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Delete(user, entity.MietminderungId);

            result.Should().BeOfType<OkResult>();
            ctx.Mietminderungen.Find(entity.MietminderungId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new MietminderungDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            var entry = new MietminderungEntryBase(entity);

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
            var service = new MietminderungDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();
            var entry = new MietminderungEntryBase(entity);

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
            var service = new MietminderungDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };

            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var entry = new MietminderungEntryBase(entity);
            entry.Ende = new DateOnly(2021, 1, 31);

            var result = await service.Put(user, entity.MietminderungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Mietminderungen.Find(entity.MietminderungId);
            if (updatedEntity == null)
            {
                throw new Exception("Mietminderung not found");
            }
            updatedEntity.Ende.Should().Be(new DateOnly(2021, 1, 31));
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new MietminderungDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            var entry = new MietminderungEntryBase(entity);
            entry.Ende = new DateOnly(2021, 1, 31);

            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.MietminderungId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
