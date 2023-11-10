using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragVersionDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragVersionDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = vertrag.Versionen.First();

            var result = service.Get(entity.VertragVersionId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<VertragVersionEntryBase>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragVersionDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = vertrag.Versionen.First();

            var result = service.Delete(entity.VertragVersionId);

            result.Should().BeOfType<OkResult>();
            ctx.VertragVersionen.Find(entity.VertragVersionId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragVersionDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new VertragVersion(new DateOnly(2021, 6, 30), 1000, 2)
            {
                Vertrag = vertrag
            };
            var entry = new VertragVersionEntryBase(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragVersionDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = vertrag.Versionen.First();
            var entry = new VertragVersionEntryBase(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragVersionDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = vertrag.Versionen.First();

            var entry = new VertragVersionEntryBase(entity);
            entry.Personenzahl = 2;

            var result = service.Put(entity.VertragVersionId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.VertragVersionen.Find(entity.VertragVersionId);
            if (updatedEntity == null)
            {
                throw new Exception("VertragVersion not found");
            }
            updatedEntity.Personenzahl.Should().Be(2);
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragVersionDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = vertrag.Versionen.First();
            var entry = new VertragVersionEntryBase(entity);
            entry.Personenzahl = 2;

            var result = service.Put(entity.VertragVersionId + 20, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}