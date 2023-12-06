using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerDbServiceTests
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
            var service = new ZaehlerDbService(ctx, auth);

            var result = await service.Get(user, vertrag.Wohnung.Zaehler.First().ZaehlerId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<ZaehlerEntry>();
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
            var service = new ZaehlerDbService(ctx, auth);

            var id = vertrag.Wohnung.Zaehler.First().ZaehlerId;
            var result = await service.Delete(user, id);

            result.Should().BeOfType<OkResult>();
            ctx.ZaehlerSet.Find(id).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ZaehlerDbService(ctx, auth);
            var entity = new Zaehler("Test", Zaehlertyp.Strom)
            {
                Wohnung = TestUtils.GetVertragForAbrechnung(ctx).Wohnung
            };
            var entry = new ZaehlerEntry(entity);

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
            var service = new ZaehlerDbService(ctx, auth);
            var entity = new Zaehler("Test", Zaehlertyp.Strom);

            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();

            var entry = new ZaehlerEntry(entity);

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
            var service = new ZaehlerDbService(ctx, auth);
            var entity = new Zaehler("Test", Zaehlertyp.Strom);
            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();
            var entry = new ZaehlerEntry(entity);
            entry.Kennnummer = "Neue Kennnummer";

            var result = await service.Put(user, entity.ZaehlerId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.ZaehlerSet.Find(entity.ZaehlerId);
            if (updatedEntity == null)
            {
                throw new Exception("Zaehler not found");
            }
            updatedEntity.Kennnummer.Should().Be("Neue Kennnummer");
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ZaehlerDbService(ctx, auth);
            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung")
            };
            var entity = new Zaehler("Test", Zaehlertyp.Strom);
            var entry = new ZaehlerEntry(entity);
            entry.Kennnummer = "Neue Kennnummer";
            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.ZaehlerId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
