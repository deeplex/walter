using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerstandControllerTests
    {
        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerstandController>>();
            var dbService = new ZaehlerstandDbService(ctx);
            var controller = new ZaehlerstandController(logger, dbService);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();

            var entity = new Zaehlerstand(new DateOnly(2021, 12, 31), 4000)
            {
                Zaehler = zaehler
            };
            var entry = new ZaehlerstandEntryBase(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ZaehlerstandController>>();
            var dbService = new ZaehlerstandDbService(ctx);
            var controller = new ZaehlerstandController(logger, dbService);

            var entity = vertrag.Wohnung.Zaehler.First().Staende.First();

            var result = controller.Get(entity.ZaehlerstandId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerstandController>>();
            var dbService = new ZaehlerstandDbService(ctx);
            var controller = new ZaehlerstandController(logger, dbService);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = vertrag.Wohnung.Zaehler.First().Staende.First();

            var entry = new ZaehlerstandEntryBase(entity);
            entry.Stand = 5000;

            var result = controller.Put(entity.ZaehlerstandId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Stand.Should().Be(5000);
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ZaehlerstandController>>();
            var dbService = new ZaehlerstandDbService(ctx);
            var controller = new ZaehlerstandController(logger, dbService);

            var entity = vertrag.Wohnung.Zaehler.First().Staende.First();
            var id = entity.ZaehlerstandId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Zaehlerstaende.Find(id).Should().BeNull();

        }
    }
}