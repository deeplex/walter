using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MieteDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MieteDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            ctx.Mieten.Add(entity);
            ctx.SaveChanges();

            var result = service.Get(entity.MieteId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<MieteEntryBase>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MieteDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            ctx.Mieten.Add(entity);
            ctx.SaveChanges();

            var result = service.Delete(entity.MieteId);

            result.Should().BeOfType<OkResult>();
            ctx.Mieten.Find(entity.MieteId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MieteDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            var entry = new MieteEntryBase(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MieteDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            ctx.Mieten.Add(entity);
            ctx.SaveChanges();
            var entry = new MieteEntryBase(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MieteDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };

            ctx.Mieten.Add(entity);
            ctx.SaveChanges();

            var entry = new MieteEntryBase(entity);
            entry.Betrag = 2000;

            var result = service.Put(entity.MieteId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Mieten.Find(entity.MieteId);
            if (updatedEntity == null)
            {
                throw new Exception("Miete not found");
            }
            updatedEntity.Betrag.Should().Be(2000);
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MieteDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            var entry = new MieteEntryBase(entity);
            entry.Betrag = 2000;

            ctx.Mieten.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.MieteId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}