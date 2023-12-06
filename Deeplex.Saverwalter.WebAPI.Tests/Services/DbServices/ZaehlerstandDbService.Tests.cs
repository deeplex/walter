using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerstandDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));

            var service = new ZaehlerstandDbService(ctx, auth);
            var zaehler = vertrag.Wohnung.Zaehler.First();

            var result = await service.Get(user, zaehler.Staende.First().ZaehlerstandId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<ZaehlerstandEntryBase>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ZaehlerstandDbService(ctx, auth);
            var zaehler = vertrag.Wohnung.Zaehler.First();

            var id = zaehler.Staende.First().ZaehlerstandId;
            var result = await service.Delete(user, id);

            result.Should().BeOfType<OkResult>();
            ctx.Zaehlerstaende.Find(id).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ZaehlerstandDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();
            var entity = new Zaehlerstand(new DateOnly(2022, 12, 31), 4000)
            {
                Zaehler = zaehler
            };
            var entry = new ZaehlerstandEntryBase(entity);

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
            var service = new ZaehlerstandDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();
            var entity = new Zaehlerstand(new DateOnly(2022, 12, 31), 4000)
            {
                Zaehler = zaehler
            };

            ctx.Zaehlerstaende.Add(entity);
            ctx.SaveChanges();

            var entry = new ZaehlerstandEntryBase(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ZaehlerstandDbService(ctx, auth);
            var zaehler = vertrag.Wohnung.Zaehler.First();
            var entity = zaehler.Staende.First();
            var entry = new ZaehlerstandEntryBase(entity);
            entry.Stand = 5000;

            var result = await service.Put(user, entity.ZaehlerstandId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Zaehlerstaende.Find(entity.ZaehlerstandId);
            if (updatedEntity == null)
            {
                throw new Exception("Zaehler not found");
            }
            updatedEntity.Stand.Should().Be(5000);
        }

        [Fact]
        public async Task PutFailedTest()
        {
            using (var ctx = TestUtils.GetContext())
            {
                var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
                var user = A.Fake<ClaimsPrincipal>();
                var auth = A.Fake<IAuthorizationService>();
                A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                    .Returns(Task.FromResult(AuthorizationResult.Success()));
                var service = new ZaehlerstandDbService(ctx, auth);
                var zaehler = vertrag.Wohnung.Zaehler.First();
                var entity = zaehler.Staende.First();
                var entry = new ZaehlerstandEntryBase(entity);
                entry.Stand = 5000;

                var result = await service.Put(user, entity.ZaehlerstandId + 31902, entry);

                result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}
