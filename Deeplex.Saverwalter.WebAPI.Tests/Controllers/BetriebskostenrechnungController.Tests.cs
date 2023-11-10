using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BetriebskostenrechnungControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var dbService = new BetriebskostenrechnungDbService(ctx);
            var controller = new BetriebskostenrechnungController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var dbService = new BetriebskostenrechnungDbService(ctx);
            var controller = new BetriebskostenrechnungController(logger, dbService);

            var umlage = new Umlage(Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachWohnflaeche);
            ctx.Umlagen.Add(umlage);
            ctx.SaveChanges();
            var entity = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };
            var entry = new BetriebskostenrechnungEntry(entity, ctx);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var dbService = new BetriebskostenrechnungDbService(ctx);
            var controller = new BetriebskostenrechnungController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Betriebskostenrechnungen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Betriebskostenrechnung is null");
            }

            var result = controller.Get(entity.BetriebskostenrechnungId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var dbService = new BetriebskostenrechnungDbService(ctx);
            var controller = new BetriebskostenrechnungController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Betriebskostenrechnungen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Betriebskostenrechnung is null");
            }
            var entry = new BetriebskostenrechnungEntry(entity, ctx);
            entry.Betrag = 2000;

            var result = controller.Put(entity.BetriebskostenrechnungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Betrag.Should().Be(2000);
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var dbService = new BetriebskostenrechnungDbService(ctx);
            var controller = new BetriebskostenrechnungController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Betriebskostenrechnungen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = entity.BetriebskostenrechnungId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Betriebskostenrechnungen.Find(id).Should().BeNull();

        }
    }
}