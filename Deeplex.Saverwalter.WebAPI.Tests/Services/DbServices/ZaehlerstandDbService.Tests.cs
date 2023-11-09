using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerstandDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ZaehlerstandDbService(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();

            var result = service.Get(zaehler.Staende.First().ZaehlerstandId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<ZaehlerstandEntryBase>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ZaehlerstandDbService(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();

            var id = zaehler.Staende.First().ZaehlerstandId;
            var result = service.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Zaehlerstaende.Find(id).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new ZaehlerstandDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();
            var entity = new Zaehlerstand(new DateOnly(2022, 12, 31), 4000)
            {
                Zaehler = zaehler
            };
            var entry = new ZaehlerstandEntryBase(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new ZaehlerstandDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();
            var entity = new Zaehlerstand(new DateOnly(2022, 12, 31), 4000)
            {
                Zaehler = zaehler
            };

            ctx.Zaehlerstaende.Add(entity);
            ctx.SaveChanges();

            var entry = new ZaehlerstandEntryBase(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ZaehlerstandDbService(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();
            var entity = zaehler.Staende.First();
            var entry = new ZaehlerstandEntryBase(entity);
            entry.Stand = 5000;

            var result = service.Put(entity.ZaehlerstandId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Zaehlerstaende.Find(entity.ZaehlerstandId);
            if (updatedEntity == null)
            {
                throw new Exception("Zaehler not found");
            }
            updatedEntity.Stand.Should().Be(5000);
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ZaehlerstandDbService(ctx);
            var zaehler = vertrag.Wohnung.Zaehler.First();
            var entity = zaehler.Staende.First();
            var entry = new ZaehlerstandEntryBase(entity);
            entry.Stand = 5000;

            var result = service.Put(entity.ZaehlerstandId + 20, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}