using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.NatuerlichePersonController;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class NatuerlichePersonControllerTests
    {
        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<NatuerlichePersonController>>();
            var dbService = new NatuerlichePersonDbService(ctx);
            var controller = new NatuerlichePersonController(logger, dbService);

            var entity = new NatuerlichePerson("TestPerson");
            var entry = new NatuerlichePersonEntry(entity, ctx);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<NatuerlichePersonController>>();
            var dbService = new NatuerlichePersonDbService(ctx);
            var controller = new NatuerlichePersonController(logger, dbService);

            var entity = new NatuerlichePerson("TestPerson");
            ctx.NatuerlichePersonen.Add(entity);
            ctx.SaveChanges();

            var result = controller.Get(entity.NatuerlichePersonId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<NatuerlichePersonController>>();
            var dbService = new NatuerlichePersonDbService(ctx);
            var controller = new NatuerlichePersonController(logger, dbService);

            var entity = new NatuerlichePerson("TestPerson");
            ctx.NatuerlichePersonen.Add(entity);
            ctx.SaveChanges();

            var entry = new NatuerlichePersonEntry(entity, ctx);
            entry.Email = "TestPerson@example.com";

            var result = controller.Put(entity.NatuerlichePersonId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entry.Email.Should().Be("TestPerson@example.com");
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<NatuerlichePersonController>>();
            var dbService = new NatuerlichePersonDbService(ctx);
            var controller = new NatuerlichePersonController(logger, dbService);

            var entity = new NatuerlichePerson("TestPerson");
            ctx.NatuerlichePersonen.Add(entity);
            ctx.SaveChanges();
            var id = entity.NatuerlichePersonId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.NatuerlichePersonen.Find(id).Should().BeNull();

        }
    }
}