using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using Deeplex.Saverwalter.Model;

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
            var adresse = new Adresse("Teststraße", "1", "12345", "Teststadt");
            var adresseEntry = new AdresseEntry(adresse, ctx);

            var result = service.Post(adresseEntry);

            result.Should().BeOfType<OkObjectResult>();
            var postedAdresse = Helper.Utils.GetAdresse(adresseEntry, ctx);
            postedAdresse.Should().NotBeNull();
        }

        [Fact]
        public void PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new AdresseDbService(ctx);
            var adresse = new Adresse("Teststraße", "1", "12345", "Teststadt");

            ctx.Adressen.Add(adresse);
            ctx.SaveChanges();

            var adresseEntry = new AdresseEntry(adresse, ctx);

            var result = service.Post(adresseEntry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void PutTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new AdresseDbService(ctx);
            var adresse = new Adresse("Teststraße", "1", "12345", "Teststadt");

            ctx.Adressen.Add(adresse);
            ctx.SaveChanges();

            var adresseEntry = new AdresseEntry(adresse, ctx);
            adresseEntry.Hausnummer = "2";

            var result = service.Put(adresse.AdresseId, adresseEntry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedAdresse = ctx.Adressen.Find(adresse.AdresseId);
            if (updatedAdresse == null)
            {
                throw new Exception("Adresse not found");
            }
            updatedAdresse.Hausnummer.Should().Be("2");
        }

        [Fact]
        public void PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var service = new AdresseDbService(ctx);
            var adresse = new Adresse("Teststraße", "1", "12345", "Teststadt");
            var adresseEntry = new AdresseEntry(adresse, ctx);
            adresseEntry.Hausnummer = "2";

            ctx.Adressen.Add(adresse);
            ctx.SaveChanges();

            var result = service.Put(adresse.AdresseId + 1, adresseEntry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}