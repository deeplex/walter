using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ErhaltungsaufwendungControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var dbService = new ErhaltungsaufwendungDbService(ctx);
            var controller = new ErhaltungsaufwendungController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var dbService = new ErhaltungsaufwendungDbService(ctx);
            var controller = new ErhaltungsaufwendungController(logger, dbService);

            var entity = new Erhaltungsaufwendung(1000, "Test", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };
            var entry = new ErhaltungsaufwendungEntry(entity, ctx);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var dbService = new ErhaltungsaufwendungDbService(ctx);
            var controller = new ErhaltungsaufwendungController(logger, dbService);

            var entity = new Erhaltungsaufwendung(1000, "Test", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = controller.Get(entity.ErhaltungsaufwendungId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var dbService = new ErhaltungsaufwendungDbService(ctx);
            var controller = new ErhaltungsaufwendungController(logger, dbService);

            var entity = new Erhaltungsaufwendung(1000, "Test", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var entry = new ErhaltungsaufwendungEntry(entity, ctx);
            entry.Betrag = 2000;

            var result = controller.Put(entity.ErhaltungsaufwendungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entry.Betrag.Should().Be(2000);
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var dbService = new ErhaltungsaufwendungDbService(ctx);
            var controller = new ErhaltungsaufwendungController(logger, dbService);

            var entity = new Erhaltungsaufwendung(1000, "Test", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();
            var id = entity.ErhaltungsaufwendungId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Erhaltungsaufwendungen.Find(id).Should().BeNull();

        }
    }
}