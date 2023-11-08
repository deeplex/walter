using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BetriebskostenrechnungDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new BetriebskostenrechnungDbService(ctx);

            var result = service.Get(
                vertrag.
                Wohnung.
                Umlagen.First().
                Betriebskostenrechnungen.First().
                BetriebskostenrechnungId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<BetriebskostenrechnungEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new BetriebskostenrechnungDbService(ctx);
            var id = vertrag.
                Wohnung.
                Umlagen.First().
                Betriebskostenrechnungen.First().
                BetriebskostenrechnungId;

            var result = service.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Betriebskostenrechnungen.Find(id).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new BetriebskostenrechnungDbService(ctx);
            var umlage = new Umlage(Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachWohnflaeche);
            ctx.Umlagen.Add(umlage);
            ctx.SaveChanges();
            var rechnung = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };
            var rechnungEntry = new BetriebskostenrechnungEntry(rechnung, ctx);

            var result = service.Post(rechnungEntry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new BetriebskostenrechnungDbService(ctx);
            var umlage = new Umlage(Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachWohnflaeche);
            var rechnung = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };

            ctx.Betriebskostenrechnungen.Add(rechnung);
            ctx.SaveChanges();

            var rechnungEntry = new BetriebskostenrechnungEntry(rechnung, ctx);

            var result = service.Post(rechnungEntry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new BetriebskostenrechnungDbService(ctx);
            var umlage = new Umlage(Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachWohnflaeche);
            var rechnung = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };
            ctx.Betriebskostenrechnungen.Add(rechnung);
            ctx.SaveChanges();
            var rechnungEntry = new BetriebskostenrechnungEntry(rechnung, ctx);
            rechnungEntry.Betrag = 2000;

            var result = service.Put(rechnung.BetriebskostenrechnungId, rechnungEntry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedRechnung = ctx.Betriebskostenrechnungen.Find(rechnung.BetriebskostenrechnungId);
            if (updatedRechnung == null)
            {
                throw new Exception("Betriebskostenrechnung not found");
            }
            updatedRechnung.Betrag.Should().Be(2000);
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new BetriebskostenrechnungDbService(ctx);
            var umlage = new Umlage(Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachWohnflaeche);
            var rechnung = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };
            var rechnungEntry = new BetriebskostenrechnungEntry(rechnung, ctx);
            ctx.Betriebskostenrechnungen.Add(rechnung);
            ctx.SaveChanges();

            var result = service.Put(rechnung.BetriebskostenrechnungId + 1, rechnungEntry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}