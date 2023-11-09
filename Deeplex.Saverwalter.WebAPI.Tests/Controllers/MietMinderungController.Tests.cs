using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MietminderungControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<MietminderungController>>();
            var dbService = new MietminderungDbService(ctx);
            var controller = new MietminderungController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<MietminderungController>>();
            var dbService = new MietminderungDbService(ctx);
            var controller = new MietminderungController(logger, dbService);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            var entry = new MietminderungEntryBase(entity);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<MietminderungController>>();
            var dbService = new MietminderungDbService(ctx);
            var controller = new MietminderungController(logger, dbService);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            if (vertrag.Mietminderungen.First() == null)
            {
                throw new NullReferenceException("Mietminderung is null");
            }

            var result = controller.Get(vertrag.Mietminderungen.First().MietminderungId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<MietminderungController>>();
            var dbService = new MietminderungDbService(ctx);
            var controller = new MietminderungController(logger, dbService);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var entry = new MietminderungEntryBase(entity);
            entry.Ende = new DateOnly(2021, 1, 31);

            var result = controller.Put(entity.MietminderungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            vertrag.Mietminderungen.First().Ende.Should().Be(new DateOnly(2021, 1, 31));
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<MietminderungController>>();
            var dbService = new MietminderungDbService(ctx);
            var controller = new MietminderungController(logger, dbService);
            var entity = new Mietminderung(new DateOnly(2021, 1, 1), 0.1)
            {
                Vertrag = vertrag
            };
            ctx.Mietminderungen.Add(entity);
            ctx.SaveChanges();

            var id = entity.MietminderungId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Mietminderungen.Find(id).Should().BeNull();

        }
    }
}