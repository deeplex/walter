using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using Deeplex.Saverwalter.Model;

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

            var entity = new Wohnung("Test", 100, 100, 1);
            var entry = new WohnungEntry(entity, ctx);

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

            var entity = new Wohnung("Test", 100, 100, 1);
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();
            var entry = new WohnungEntry(entity, ctx);
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