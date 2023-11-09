using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MieteControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<MieteController>>();
            var dbService = new MieteDbService(ctx);
            var controller = new MieteController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<MieteController>>();
            var dbService = new MieteDbService(ctx);
            var controller = new MieteController(logger, dbService);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            var entry = new MieteEntryBase(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var dbService = new MieteDbService(ctx);
            var controller = new MieteController(logger, dbService);

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Miete is null");
            }

            var result = controller.Get(vertrag.Mieten.First().MieteId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var dbService = new MieteDbService(ctx);
            var controller = new MieteController(logger, dbService);

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Miete is null");
            }
            var entry = new MieteEntryBase(vertrag.Mieten.First());
            entry.Betrag = 2000;

            var result = controller.Put(vertrag.Mieten.First().MieteId, entry);

            result.Should().BeOfType<OkObjectResult>();
            vertrag.Mieten.First().Betrag.Should().Be(2000);
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var dbService = new MieteDbService(ctx);
            var controller = new MieteController(logger, dbService);

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = vertrag.Mieten.First().MieteId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Mieten.Find(id).Should().BeNull();

        }
    }
}