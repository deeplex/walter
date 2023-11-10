using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragDbService(ctx);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var result = service.Get(entity.VertragId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<VertragEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragDbService(ctx);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var result = service.Delete(entity.VertragId);

            result.Should().BeOfType<OkResult>();
            ctx.Vertraege.Find(entity.VertragId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Vertrag()
            {
                Wohnung = vertrag.Wohnung
            };
            var entry = new VertragEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragDbService(ctx);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var entry = new VertragEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragDbService(ctx);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);

            var entry = new VertragEntry(entity, ctx);
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = service.Put(entity.VertragId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Vertraege.Find(entity.VertragId);
            if (updatedEntity == null)
            {
                throw new Exception("Vertrag not found");
            }
            updatedEntity.Ende.Should().Be(new DateOnly(2021, 12, 31));
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new VertragDbService(ctx);
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var entry = new VertragEntry(entity, ctx);
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = service.Put(entity.VertragId + 20, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}