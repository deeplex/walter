using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MietminderungDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MietminderungDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var result = service.Get(entity.MietminderungId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<MietminderungEntryBase>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MietminderungDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var result = service.Delete(entity.MietminderungId);

            result.Should().BeOfType<OkResult>();
            ctx.Mietminderungen.Find(entity.MietminderungId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MietminderungDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            var entry = new MietminderungEntryBase(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MietminderungDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();
            var entry = new MietminderungEntryBase(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MietminderungDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };

            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var entry = new MietminderungEntryBase(entity);
            entry.Ende = new DateOnly(2021, 1, 31);

            var result = service.Put(entity.MietminderungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Mietminderungen.Find(entity.MietminderungId);
            if (updatedEntity == null)
            {
                throw new Exception("Mietminderung not found");
            }
            updatedEntity.Ende.Should().Be(new DateOnly(2021, 1, 31));
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new MietminderungDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            var entry = new MietminderungEntryBase(entity);
            entry.Ende = new DateOnly(2021, 1, 31);

            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.MietminderungId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}