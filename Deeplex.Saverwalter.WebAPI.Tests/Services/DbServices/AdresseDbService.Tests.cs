using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AdresseDbServiceTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new AdresseDbService(ctx);
            if (vertrag.Wohnung.Adresse == null)
            {
                throw new Exception("Adresse not found");
            }

            var result = service.Get(vertrag.Wohnung.Adresse.AdresseId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<AdresseEntry>();
        }

        [Fact]
        public void DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var service = new AdresseDbService(ctx);
            if (vertrag.Wohnung.Adresse == null)
            {
                throw new Exception("Adresse not found");
            }
            var id = vertrag.Wohnung.Adresse.AdresseId;

            var result = service.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Adressen.Find(id).Should().BeNull();
        }

        [Fact]
        public void PostTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new AdresseDbService(ctx);
            var entity = new Adresse("Teststraße", "1", "12345", "Teststadt");
            var entry = new AdresseEntry(entity);

            var result = service.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
            var postedAdresse = Helper.Utils.GetAdresse(entry, ctx);
            postedAdresse.Should().NotBeNull();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new AdresseDbService(ctx);
            var entity = new Adresse("Teststraße", "1", "12345", "Teststadt");

            ctx.Adressen.Add(entity);
            ctx.SaveChanges();

            var adresseEntry = new AdresseEntry(entity);

            var result = service.Post(adresseEntry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new AdresseDbService(ctx);
            var entity = new Adresse("Teststraße", "1", "12345", "Teststadt");

            ctx.Adressen.Add(entity);
            ctx.SaveChanges();

            var entry = new AdresseEntry(entity);
            entry.Hausnummer = "2";

            var result = service.Put(entity.AdresseId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Adressen.Find(entity.AdresseId);
            if (updatedEntity == null)
            {
                throw new Exception("Adresse not found");
            }
            updatedEntity.Hausnummer.Should().Be("2");
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new AdresseDbService(ctx);
            var entity = new Adresse("Teststraße", "1", "12345", "Teststadt");
            var entry = new AdresseEntry(entity);
            entry.Hausnummer = "2";

            ctx.Adressen.Add(entity);
            ctx.SaveChanges();

            var result = service.Put(entity.AdresseId + 20, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}