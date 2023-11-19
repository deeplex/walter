using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlagetypControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var dbService = new UmlagetypDbService(ctx);
            var controller = new UmlagetypController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var dbService = new UmlagetypDbService(ctx);
            var controller = new UmlagetypController(logger, dbService);

            var entity = new Umlagetyp("Hausstrom");
            var entry = new UmlagetypEntry(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var dbService = new UmlagetypDbService(ctx);
            var controller = new UmlagetypController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Umlagetyp is null");
            }

            var result = controller.Get(entity.UmlagetypId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var dbService = new UmlagetypDbService(ctx);
            var controller = new UmlagetypController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Umlagetyp is null");
            }
            var entry = new UmlagetypEntry(entity);
            entry.Bezeichnung = "Test";

            var result = controller.Put(entity.UmlagetypId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Bezeichnung.Should().Be("Test");
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var dbService = new UmlagetypDbService(ctx);
            var controller = new UmlagetypController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = entity.UmlagetypId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagetypen.Find(id).Should().BeNull();

        }
    }
}