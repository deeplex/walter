using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragVersionenControllerTests
    {
        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<VertragVersionController>>();
            var dbService = new VertragVersionDbService(ctx);
            var controller = new VertragVersionController(logger, dbService);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = new VertragVersion(new DateOnly(2021, 6, 30), 1000, 2)
            {
                Vertrag = vertrag
            };
            var entry = new VertragVersionEntryBase(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragVersionController>>();
            var dbService = new VertragVersionDbService(ctx);
            var controller = new VertragVersionController(logger, dbService);
            var entity = vertrag.Versionen.First();

            if (entity == null)
            {
                throw new NullReferenceException("Vertrag has no Versionen");
            }

            var result = controller.Get(entity.VertragVersionId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragVersionController>>();
            var dbService = new VertragVersionDbService(ctx);
            var controller = new VertragVersionController(logger, dbService);
            var entity = vertrag.Versionen.First();

            if (entity == null)
            {
                throw new NullReferenceException("Vertrag has no Versionen");
            }

            var entry = new VertragVersionEntryBase(entity);
            entry.Personenzahl = 4;

            var result = controller.Put(entity.VertragVersionId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Personenzahl.Should().Be(4);
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragVersionController>>();
            var dbService = new VertragVersionDbService(ctx);
            var controller = new VertragVersionController(logger, dbService);
            var entity = vertrag.Versionen.First();

            var id = entity.VertragVersionId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.VertragVersionen.Find(id).Should().BeNull();

        }
    }
}