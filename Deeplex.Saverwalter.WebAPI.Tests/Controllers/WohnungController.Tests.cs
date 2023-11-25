using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class WohnungControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<WohnungController>>();
            var dbService = new WohnungDbService(ctx);
            var controller = new WohnungController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<WohnungController>>();
            var dbService = new WohnungDbService(ctx);
            var controller = new WohnungController(logger, dbService);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);
            ctx.SaveChanges();

            var entity = new Wohnung("Test", 100, 100, 1)
            {
                Besitzer = besitzer
            };
            var entry = new WohnungEntry(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<WohnungController>>();
            var dbService = new WohnungDbService(ctx);
            var controller = new WohnungController(logger, dbService);

            var entity = vertrag.Wohnung;

            var result = controller.Get(entity.WohnungId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<WohnungController>>();
            var dbService = new WohnungDbService(ctx);
            var controller = new WohnungController(logger, dbService);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var entity = new Wohnung("Test", 100, 100, 1)
            {
                Besitzer = besitzer
            };
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();
            var entry = new WohnungEntry(entity);
            entry.Wohnflaeche = 200;

            var result = controller.Put(entity.WohnungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Wohnflaeche.Should().Be(200);
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<WohnungController>>();
            var dbService = new WohnungDbService(ctx);
            var controller = new WohnungController(logger, dbService);

            var entity = vertrag.Wohnung;
            var id = entity.WohnungId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Wohnungen.Find(id).Should().BeNull();

        }
    }
}