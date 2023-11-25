using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlageControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<UmlageController>>();
            var dbService = new UmlageDbService(ctx);
            var controller = new UmlageController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<UmlageController>>();
            var dbService = new UmlageDbService(ctx);
            var controller = new UmlageController(logger, dbService);

            var typ = new Umlagetyp("Dachrinnenreinigung");
            ctx.Umlagetypen.Add(typ);
            ctx.SaveChanges();

            var entity = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = typ
            };
            var entry = new UmlageEntry(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlageController>>();
            var dbService = new UmlageDbService(ctx);
            var controller = new UmlageController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Umlage is null");
            }

            var result = controller.Get(entity.UmlageId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlageController>>();
            var dbService = new UmlageDbService(ctx);
            var controller = new UmlageController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Umlage is null");
            }
            var entry = new UmlageEntry(entity);
            entry.Beschreibung = "Test";

            var result = controller.Put(entity.UmlageId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Beschreibung.Should().Be("Test");
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlageController>>();
            var dbService = new UmlageDbService(ctx);
            var controller = new UmlageController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = entity.UmlageId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagen.Find(id).Should().BeNull();

        }
    }
}