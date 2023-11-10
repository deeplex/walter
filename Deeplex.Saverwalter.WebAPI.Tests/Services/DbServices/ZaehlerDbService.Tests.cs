using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZaehlerDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ZaehlerDbService(ctx);

            var result = service.Get(vertrag.Wohnung.Zaehler.First().ZaehlerId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<ZaehlerEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ZaehlerDbService(ctx);

            var id = vertrag.Wohnung.Zaehler.First().ZaehlerId;
            var result = service.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.ZaehlerSet.Find(id).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new ZaehlerDbService(ctx);
            var entity = new Zaehler("Test", Zaehlertyp.Strom);
            var entry = new ZaehlerEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new ZaehlerDbService(ctx);
            var entity = new Zaehler("Test", Zaehlertyp.Strom);

            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();

            var entry = new ZaehlerEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new ZaehlerDbService(ctx);
            var entity = new Zaehler("Test", Zaehlertyp.Strom);
            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();
            var entry = new ZaehlerEntry(entity);
            entry.Kennnummer = "Neue Kennnummer";

            var result = service.Put(entity.ZaehlerId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.ZaehlerSet.Find(entity.ZaehlerId);
            if (updatedEntity == null)
            {
                throw new Exception("Zaehler not found");
            }
            updatedEntity.Kennnummer.Should().Be("Neue Kennnummer");
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new ZaehlerDbService(ctx);
            var umlage = new Umlage(Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachWohnflaeche);
            var entity = new Zaehler("Test", Zaehlertyp.Strom);
            var entry = new ZaehlerEntry(entity);
            entry.Kennnummer = "Neue Kennnummer";
            ctx.ZaehlerSet.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.ZaehlerId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}