using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlagetypDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlagetypDbService(ctx);
            var entity = new Umlagetyp("Hausstrom");
            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();

            var result = service.Get(entity.UmlagetypId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<UmlagetypEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlagetypDbService(ctx);
            var entity = new Umlagetyp("Hausstrom");
            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();

            var result = service.Delete(entity.UmlagetypId);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagen.Find(entity.UmlagetypId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlagetypDbService(ctx);
            var entity = new Umlagetyp("Hausstrom");
            var entry = new UmlagetypEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlagetypDbService(ctx);
            var entity = new Umlagetyp("Hausstrom");

            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();
            var entry = new UmlagetypEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlagetypDbService(ctx);
            var entity = new Umlagetyp("Hausstrom");

            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();

            var entry = new UmlagetypEntry(entity);
            entry.Bezeichnung = "Test";

            var result = service.Put(entity.UmlagetypId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Umlagetypen.Find(entity.UmlagetypId);
            if (updatedEntity == null)
            {
                throw new Exception("Umlagetyp not found");
            }
            updatedEntity.Bezeichnung.Should().Be("Test");
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlagetypDbService(ctx);
            var entity = new Umlagetyp("Hausstrom");
            var entry = new UmlagetypEntry(entity);
            entry.Bezeichnung = "Test";

            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.UmlagetypId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}