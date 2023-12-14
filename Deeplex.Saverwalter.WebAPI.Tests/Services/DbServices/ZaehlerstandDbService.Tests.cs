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

            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<ZaehlerstandEntry>();
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
            var entry = new ZaehlerstandEntry(entity, new());

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
            var service = new ZaehlerstandDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();
            var entity = new Zaehlerstand(new DateOnly(2022, 12, 31), 4000)
            {
                Zaehler = zaehler
            };

            ctx.Zaehlerstaende.Add(entity);
            ctx.SaveChanges();

            var entry = new ZaehlerstandEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Result.Should().BeOfType<BadRequestResult>();
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
            var entry = new ZaehlerstandEntry(entity, new());
            entry.Stand = 5000;

            var result = await service.Put(user, entity.ZaehlerstandId, entry);

            result.Value.Should().NotBeNull();
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
                var entry = new ZaehlerstandEntry(entity, new());
                entry.Stand = 5000;

                var result = await service.Put(user, entity.ZaehlerstandId + 31902, entry);

                result.Result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}
