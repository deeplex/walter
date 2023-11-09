using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ErhaltungsaufwednungDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ErhaltungsaufwendungDbService(ctx);
            var aufwendung = new Erhaltungsaufwendung(
                1000, "TestAufwendung", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(aufwendung);
            ctx.SaveChanges();

            var result = service.Get(aufwendung.ErhaltungsaufwendungId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<ErhaltungsaufwendungEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ErhaltungsaufwendungDbService(ctx);
            var aufwendung = new Erhaltungsaufwendung(
                1000, "TestAufwendung", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1));
            ctx.Erhaltungsaufwendungen.Add(aufwendung);
            ctx.SaveChanges();

            var result = service.Delete(aufwendung.ErhaltungsaufwendungId);

            result.Should().BeOfType<OkResult>();
            ctx.Erhaltungsaufwendungen.Find(aufwendung.ErhaltungsaufwendungId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ErhaltungsaufwendungDbService(ctx);
            var aufwendung = new Erhaltungsaufwendung(
                1000, "TestAufwendung", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };
            var aufwendungEntry = new ErhaltungsaufwendungEntry(aufwendung, ctx);

            var result = service.Post(aufwendungEntry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ErhaltungsaufwendungDbService(ctx);
            var aufwendung = new Erhaltungsaufwendung(
                1000, "TestAufwendung", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(aufwendung);
            ctx.SaveChanges();
            var aufwendungEntry = new ErhaltungsaufwendungEntry(aufwendung, ctx);

            var result = service.Post(aufwendungEntry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ErhaltungsaufwendungDbService(ctx);
            var aufwendung = new Erhaltungsaufwendung(
                1000, "TestAufwendung", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };

            ctx.Erhaltungsaufwendungen.Add(aufwendung);
            ctx.SaveChanges();

            var aufwendungEntry = new ErhaltungsaufwendungEntry(aufwendung, ctx);
            aufwendungEntry.Betrag = 2000;

            var result = service.Put(aufwendung.ErhaltungsaufwendungId, aufwendungEntry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedAufwendung = ctx.Erhaltungsaufwendungen.Find(aufwendung.ErhaltungsaufwendungId);
            if (updatedAufwendung == null)
            {
                throw new Exception("Erhaltungsaufwendung not found");
            }
            updatedAufwendung.Betrag.Should().Be(2000);
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new ErhaltungsaufwendungDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var aufwendung = new Erhaltungsaufwendung(
                1000, "TestAufwendung", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };
            var aufwendungEntry = new ErhaltungsaufwendungEntry(aufwendung, ctx);
            aufwendungEntry.Betrag = 2000;

            ctx.Erhaltungsaufwendungen.Add(aufwendung);
            ctx.SaveChanges();

            var result = service.Put(aufwendung.ErhaltungsaufwendungId + 1, aufwendungEntry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}