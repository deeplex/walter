using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class JuristischePersonDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new KontaktDbService(ctx);
            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = service.Get(entity.KontaktId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<KontaktEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new KontaktDbService(ctx);
            var entity = new Kontakt("TestFirma", Rechtsform.gbr);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = service.Delete(entity.KontaktId);

            result.Should().BeOfType<OkResult>();
            ctx.Kontakte.Find(entity.KontaktId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new KontaktDbService(ctx);
            var entity = new Kontakt("TestFirma", Rechtsform.gmbh);
            var entry = new KontaktEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new KontaktDbService(ctx);
            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();
            var entry = new KontaktEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new KontaktDbService(ctx);
            var entity = new Kontakt("TestFirma", Rechtsform.gmbh);

            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var entry = new KontaktEntry(entity);
            entry.Email = "testfirma@saverwalter.de";

            var result = service.Put(entity.KontaktId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Kontakte.Find(entity.KontaktId);
            if (updatedEntity == null)
            {
                throw new Exception("JuristischePerson not found");
            }
            updatedEntity.Email.Should().Be("testfirma@saverwalter.de");
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new KontaktDbService(ctx);
            var entity = new Kontakt("TestFirma", Rechtsform.ag);
            var entry = new KontaktEntry(entity);
            entry.Email = "testfirma@saverwalter.de";

            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.KontaktId + 1132, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}