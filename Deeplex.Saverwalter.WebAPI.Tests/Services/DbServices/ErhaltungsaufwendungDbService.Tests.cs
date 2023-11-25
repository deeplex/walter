using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

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

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = service.Get(entity.ErhaltungsaufwendungId);

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

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = service.Delete(entity.ErhaltungsaufwendungId);

            result.Should().BeOfType<OkResult>();
            ctx.Erhaltungsaufwendungen.Find(entity.ErhaltungsaufwendungId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ErhaltungsaufwendungDbService(ctx);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            var entry = new ErhaltungsaufwendungEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ErhaltungsaufwendungDbService(ctx);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();
            var entry = new ErhaltungsaufwendungEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new ErhaltungsaufwendungDbService(ctx);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };

            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var entry = new ErhaltungsaufwendungEntry(entity);
            entry.Betrag = 2000;

            var result = service.Put(entity.ErhaltungsaufwendungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Erhaltungsaufwendungen.Find(entity.ErhaltungsaufwendungId);
            if (updatedEntity == null)
            {
                throw new Exception("Erhaltungsaufwendung not found");
            }
            updatedEntity.Betrag.Should().Be(2000);
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new ErhaltungsaufwendungDbService(ctx);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var aussteller = new Kontakt("TestPerson", Rechtsform.ag);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            var entry = new ErhaltungsaufwendungEntry(entity);
            entry.Betrag = 2000;

            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.ErhaltungsaufwendungId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}