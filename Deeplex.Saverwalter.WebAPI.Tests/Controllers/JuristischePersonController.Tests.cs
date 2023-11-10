using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.JuristischePersonController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class JuristischePersonControllerTests
    {
        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<JuristischePersonController>>();
            var dbService = new JuristischePersonDbService(ctx);
            var controller = new JuristischePersonController(logger, dbService);

            var entity = new JuristischePerson("TestFirma");
            var entry = new JuristischePersonEntry(entity, ctx);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<JuristischePersonController>>();
            var dbService = new JuristischePersonDbService(ctx);
            var controller = new JuristischePersonController(logger, dbService);

            var entity = new JuristischePerson("TestFirma");
            ctx.JuristischePersonen.Add(entity);
            ctx.SaveChanges();

            var result = controller.Get(entity.JuristischePersonId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<JuristischePersonController>>();
            var dbService = new JuristischePersonDbService(ctx);
            var controller = new JuristischePersonController(logger, dbService);

            var entity = new JuristischePerson("TestFirma");
            ctx.JuristischePersonen.Add(entity);
            ctx.SaveChanges();

            var entry = new JuristischePersonEntry(entity, ctx);
            entry.Email = "TestFirma@example.com";

            var result = controller.Put(entity.JuristischePersonId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entry.Email.Should().Be("TestFirma@example.com");
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<JuristischePersonController>>();
            var dbService = new JuristischePersonDbService(ctx);
            var controller = new JuristischePersonController(logger, dbService);

            var entity = new JuristischePerson("TestFirma");
            ctx.JuristischePersonen.Add(entity);
            ctx.SaveChanges();
            var id = entity.JuristischePersonId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.JuristischePersonen.Find(id).Should().BeNull();

        }
    }
}