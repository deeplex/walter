using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerDbService(ctx, auth);
            var controller = new ZaehlerController(logger, dbService);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();

            var result = await controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerDbService(ctx, auth);
            var controller = new ZaehlerController(logger, dbService);

            var entity = new Zaehler("Test", Zaehlertyp.Warmwasser)
            {
                Wohnung = TestUtils.GetVertragForAbrechnung(ctx).Wohnung
            };
            var entry = new ZaehlerEntry(entity);

            var result = await controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerDbService(ctx, auth);
            var controller = new ZaehlerController(logger, dbService);

            var entity = vertrag.Wohnung.Zaehler.First();

            var result = await controller.Get(entity.ZaehlerId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerDbService(ctx, auth);
            var controller = new ZaehlerController(logger, dbService);

            var entity = new Zaehler("Test", Zaehlertyp.Warmwasser);
            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();
            var entry = new ZaehlerEntry(entity);
            entry.Kennnummer = "ChangedNummer";

            var result = await controller.Put(entity.ZaehlerId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Kennnummer.Should().Be("ChangedNummer");
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ZaehlerDbService(ctx, auth);
            var controller = new ZaehlerController(logger, dbService);

            var entity = vertrag.Wohnung.Zaehler.First();
            var id = entity.ZaehlerId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.ZaehlerSet.Find(id).Should().BeNull();

        }
    }
}
