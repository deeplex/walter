using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<VertragController>>();
            var dbService = new VertragDbService(ctx);
            var controller = new VertragController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<VertragController>>();
            var dbService = new VertragDbService(ctx);
            var controller = new VertragController(logger, dbService);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var wohnung = new Wohnung("Test", 100, 100, 1)
            {
                Besitzer = besitzer
            };
            ctx.Wohnungen.Add(wohnung);
            ctx.SaveChanges();

            var entity = new Vertrag()
            {
                Ansprechpartner = wohnung.Besitzer,
                Wohnung = wohnung
            };
            var entry = new VertragEntry(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var dbService = new VertragDbService(ctx);
            var controller = new VertragController(logger, dbService);

            var result = controller.Get(entity.VertragId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var dbService = new VertragDbService(ctx);
            var controller = new VertragController(logger, dbService);

            var entry = new VertragEntry(entity);
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = controller.Put(entity.VertragId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Ende.Should().Be(new DateOnly(2021, 12, 31));
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var dbService = new VertragDbService(ctx);
            var controller = new VertragController(logger, dbService);

            var id = entity.VertragId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Vertraege.Find(id).Should().BeNull();

        }
    }
}