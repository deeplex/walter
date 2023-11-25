using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class JuristischePersonControllerTests
    {
        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var dbService = new KontaktDbService(ctx);
            var controller = new KontaktController(logger, dbService);

            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            var entry = new KontaktEntry(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var dbService = new KontaktDbService(ctx);
            var controller = new KontaktController(logger, dbService);

            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = controller.Get(entity.KontaktId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var dbService = new KontaktDbService(ctx);
            var controller = new KontaktController(logger, dbService);

            var entity = new Kontakt("TestFirma", Rechtsform.gmbh);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var entry = new KontaktEntry(entity);
            entry.Email = "TestFirma@example.com";

            var result = controller.Put(entity.KontaktId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entry.Email.Should().Be("TestFirma@example.com");
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var dbService = new KontaktDbService(ctx);
            var controller = new KontaktController(logger, dbService);

            var entity = new Kontakt("TestFirma", Rechtsform.gbr);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();
            var id = entity.KontaktId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Kontakte.Find(id).Should().BeNull();

        }
    }
}