using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.NatuerlichePersonController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class NatuerlichePersonDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new NatuerlichePersonDbService(ctx);
            var entity = new NatuerlichePerson("TestPerson");
            ctx.NatuerlichePersonen.Add(entity);
            ctx.SaveChanges();

            var result = service.Get(entity.NatuerlichePersonId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<NatuerlichePersonEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new NatuerlichePersonDbService(ctx);
            var entity = new NatuerlichePerson("TestPerson");
            ctx.NatuerlichePersonen.Add(entity);
            ctx.SaveChanges();

            var result = service.Delete(entity.NatuerlichePersonId);

            result.Should().BeOfType<OkResult>();
            ctx.NatuerlichePersonen.Find(entity.NatuerlichePersonId).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new NatuerlichePersonDbService(ctx);
            var entity = new NatuerlichePerson("TestPerson");
            var entry = new NatuerlichePersonEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new NatuerlichePersonDbService(ctx);
            var entity = new NatuerlichePerson("TestPerson");
            ctx.NatuerlichePersonen.Add(entity);
            ctx.SaveChanges();
            var entry = new NatuerlichePersonEntry(entity, ctx);

            var result = service.Post(entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new NatuerlichePersonDbService(ctx);
            var entity = new NatuerlichePerson("TestPerson");

            ctx.NatuerlichePersonen.Add(entity);
            ctx.SaveChanges();

            var entry = new NatuerlichePersonEntry(entity, ctx);
            entry.Email = "TestPerson@saverwalter.de";

            var result = service.Put(entity.NatuerlichePersonId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.NatuerlichePersonen.Find(entity.NatuerlichePersonId);
            if (updatedEntity == null)
            {
                throw new Exception("Erhaltungsaufwendung not found");
            }
            updatedEntity.Email.Should().Be("TestPerson@saverwalter.de");
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new NatuerlichePersonDbService(ctx);
            var entity = new NatuerlichePerson("TestPerson");
            var entry = new NatuerlichePersonEntry(entity, ctx);
            entry.Email = "TestPerson@saverwalter.de";

            ctx.NatuerlichePersonen.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.NatuerlichePersonId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}