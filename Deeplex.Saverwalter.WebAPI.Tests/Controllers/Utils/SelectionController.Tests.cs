using System.Security.Claims;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class SelectionControllerTests
    {
        [Fact]
        public async Task GetAdressen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetAdressen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetBetriebskostenrechnungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetBetriebskostenrechnungen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetErhaltungsaufwendungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetErhaltungsaufwendungen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMieten()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetMieten();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMietminderungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetMietminderungen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUmlagen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetUmlagen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUmlagenWohnungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetUmlagenWohnungen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUmlagenVerbrauch()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetUmlagenVerbrauch();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetKontakte()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = controller.GetKontakte();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetJuristischePersonen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = controller.GetJuristischePersonen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetWohnungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetWohnungen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetVertraege()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetVertraege();

            result.Result.Should().BeOfType<OkObjectResult>();
        }


        [Fact]
        public async Task GetZaehler()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetZaehler();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetZaehlerStaende()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetZaehlerStaende();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUmlagetypen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.GetUmlagetypen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUmlageschluessel()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = controller.GetUmlageschluessel();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetHKVO_P9A2()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = controller.GetHKVO_P9A2();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetZaehlertypen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = controller.GetZaehlertypen();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetAnreden()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = controller.GetAnreden();

            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}
