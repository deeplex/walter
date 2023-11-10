using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var dbService = new ZaehlerDbService(ctx);
            var controller = new ZaehlerController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var dbService = new ZaehlerDbService(ctx);
            var controller = new ZaehlerController(logger, dbService);

            var entity = new Zaehler("Test", Zaehlertyp.Warmwasser);
            var entry = new ZaehlerEntry(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var dbService = new ZaehlerDbService(ctx);
            var controller = new ZaehlerController(logger, dbService);

            var entity = vertrag.Wohnung.Zaehler.First();

            var result = controller.Get(entity.ZaehlerId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var dbService = new ZaehlerDbService(ctx);
            var controller = new ZaehlerController(logger, dbService);

            var entity = new Zaehler("Test", Zaehlertyp.Warmwasser);
            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();
            var entry = new ZaehlerEntry(entity);
            entry.Kennnummer = "ChangedNummer";

            var result = controller.Put(entity.ZaehlerId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Kennnummer.Should().Be("ChangedNummer");
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ZaehlerController>>();
            var dbService = new ZaehlerDbService(ctx);
            var controller = new ZaehlerController(logger, dbService);

            var entity = vertrag.Wohnung.Zaehler.First();
            var id = entity.ZaehlerId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.ZaehlerSet.Find(id).Should().BeNull();

        }
    }
}