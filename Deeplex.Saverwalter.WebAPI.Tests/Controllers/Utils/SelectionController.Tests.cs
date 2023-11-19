using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class SelectionControllerTests
    {
        [Fact]
        public void GetAdressen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetAdressen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetBetriebskostenrechnungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetBetriebskostenrechnungen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetErhaltungsaufwendungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetErhaltungsaufwendungen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetMieten()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetMieten();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetMietminderungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetMietminderungen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUmlagen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetUmlagen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUmlagenWohnungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetUmlagenWohnungen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUmlagenVerbrauch()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetUmlagenVerbrauch();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetKontakte()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetKontakte();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetNatuerlichePersonen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetNatuerlichePersonen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetJuristischePersonen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetJuristischePersonen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetWohnungen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetWohnungen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetVertraege()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetVertraege();

            result.Should().BeOfType<OkObjectResult>();
        }


        [Fact]
        public void GetZaehler()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetZaehler();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetZaehlerStaende()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetZaehlerStaende();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUmlagetypen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetUmlagetypen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUmlageschluessel()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetUmlageschluessel();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetHKVO_P9A2()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetHKVO_P9A2();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetZaehlertypen()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetZaehlertypen();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetAnreden()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<SelectionListController>>();
            var controller = new SelectionListController(logger, ctx);

            var result = controller.GetAnreden();

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}