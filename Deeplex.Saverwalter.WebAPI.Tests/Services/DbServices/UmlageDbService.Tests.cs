using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlageDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlageDbService(ctx);
            var entity = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachWohnflaeche);
            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();

            var result = service.Get(entity.UmlageId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<UmlageEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlageDbService(ctx);
            var entity = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachWohnflaeche);
            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();

            var result = service.Delete(entity.UmlageId);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagen.Find(entity.UmlageId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlageDbService(ctx);
            var entity = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachWohnflaeche);
            var entry = new UmlageEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlageDbService(ctx);
            var entity = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachWohnflaeche);
            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();
            var entry = new UmlageEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlageDbService(ctx);
            var entity = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachWohnflaeche);

            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();

            var entry = new UmlageEntry(entity, ctx);
            entry.Beschreibung = "Test";

            var result = service.Put(entity.UmlageId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Umlagen.Find(entity.UmlageId);
            if (updatedEntity == null)
            {
                throw new Exception("Umlage not found");
            }
            updatedEntity.Beschreibung.Should().Be("Test");
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new UmlageDbService(ctx);
            var entity = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachWohnflaeche);
            var entry = new UmlageEntry(entity, ctx);
            entry.Beschreibung = "Test";

            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.UmlageId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}