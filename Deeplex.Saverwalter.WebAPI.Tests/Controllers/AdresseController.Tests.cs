using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AdresseControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<AdresseController>>();
            var dbService = new AdresseDbService(ctx);
            var controller = new AdresseController(logger, dbService);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<AdresseController>>();
            var dbService = new AdresseDbService(ctx);
            var controller = new AdresseController(logger, dbService);

            var entity = new Adresse("Teststrasse", "1", "12345", "Teststadt");
            var entry = new AdresseEntry(entity, ctx);

            var result = controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            var dbService = new AdresseDbService(ctx);
            var controller = new AdresseController(logger, dbService);

            if (vertrag.Wohnung.Adresse == null)
            {
                throw new NullReferenceException("Adresse is null");
            }

            var result = controller.Get(vertrag.Wohnung.Adresse.AdresseId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            var dbService = new AdresseDbService(ctx);
            var controller = new AdresseController(logger, dbService);

            if (vertrag.Wohnung.Adresse == null)
            {
                throw new NullReferenceException("Adresse is null");
            }
            var entry = new AdresseEntry(vertrag.Wohnung.Adresse, ctx);
            entry.Hausnummer = "2";

            var result = controller.Put(vertrag.Wohnung.Adresse.AdresseId, entry);

            result.Should().BeOfType<OkObjectResult>();
            vertrag.Wohnung.Adresse.Hausnummer.Should().Be("2");
        }

        [Fact]
        public void Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            var dbService = new AdresseDbService(ctx);
            var controller = new AdresseController(logger, dbService);

            if (vertrag.Wohnung.Adresse == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = vertrag.Wohnung.Adresse.AdresseId;

            var result = controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Adressen.Find(id).Should().BeNull();

        }
    }
}