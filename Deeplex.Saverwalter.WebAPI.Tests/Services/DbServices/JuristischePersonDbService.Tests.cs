using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.JuristischePersonController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class JuristischePersonDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new JuristischePersonDbService(ctx);
            var entity = new JuristischePerson("TestFirma");
            ctx.JuristischePersonen.Add(entity);
            ctx.SaveChanges();

            var result = service.Get(entity.JuristischePersonId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<JuristischePersonEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new JuristischePersonDbService(ctx);
            var entity = new JuristischePerson("TestFirma");
            ctx.JuristischePersonen.Add(entity);
            ctx.SaveChanges();

            var result = service.Delete(entity.JuristischePersonId);

            result.Should().BeOfType<OkResult>();
            ctx.JuristischePersonen.Find(entity.JuristischePersonId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new JuristischePersonDbService(ctx);
            var entity = new JuristischePerson("TestFirma");
            var entry = new JuristischePersonEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new JuristischePersonDbService(ctx);
            var entity = new JuristischePerson("TestFirma");
            ctx.JuristischePersonen.Add(entity);
            ctx.SaveChanges();
            var entry = new JuristischePersonEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new JuristischePersonDbService(ctx);
            var entity = new JuristischePerson("TestFirma");

            ctx.JuristischePersonen.Add(entity);
            ctx.SaveChanges();

            var entry = new JuristischePersonEntry(entity, ctx);
            entry.Email = "testfirma@saverwalter.de";

            var result = service.Put(entity.JuristischePersonId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.JuristischePersonen.Find(entity.JuristischePersonId);
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
            var service = new JuristischePersonDbService(ctx);
            var entity = new JuristischePerson("TestFirma");
            var entry = new JuristischePersonEntry(entity, ctx);
            entry.Email = "testfirma@saverwalter.de";

            ctx.JuristischePersonen.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.JuristischePersonId + 1132, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}