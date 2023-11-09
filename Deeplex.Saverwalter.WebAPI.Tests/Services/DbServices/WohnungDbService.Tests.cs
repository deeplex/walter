using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class WohnungDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new WohnungDbService(ctx);

            var result = service.Get(vertrag.Wohnung.WohnungId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<WohnungEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new WohnungDbService(ctx);

            var id = vertrag.Wohnung.WohnungId;
            var result = service.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Wohnungen.Find(id).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new WohnungDbService(ctx);
            var entity = new Wohnung("Test", 100, 100, 1);
            var entry = new WohnungEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new WohnungDbService(ctx);
            var entity = new Wohnung("Test", 100, 100, 1);

            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();

            var entry = new WohnungEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new WohnungDbService(ctx);
            var entity = new Wohnung("Test", 100, 100, 1);
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();
            var entry = new WohnungEntry(entity, ctx);
            entry.Wohnflaeche = 200;

            var result = service.Put(entity.WohnungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Wohnungen.Find(entity.WohnungId);
            if (updatedEntity == null)
            {
                throw new Exception("Wohnung not found");
            }
            updatedEntity.Wohnflaeche.Should().Be(200);
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new WohnungDbService(ctx);
            var umlage = new Umlage(Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachWohnflaeche);
            var entity = new Wohnung("Test", 100, 100, 1);
            var entry = new WohnungEntry(entity, ctx);
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.WohnungId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}