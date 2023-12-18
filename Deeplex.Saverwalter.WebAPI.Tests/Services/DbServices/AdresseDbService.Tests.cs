using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AdresseDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new AdresseDbService(ctx, auth);
            if (vertrag.Wohnung.Adresse == null)
            {
                throw new Exception("Adresse not found");
            }

            var result = await service.Get(user, vertrag.Wohnung.Adresse.AdresseId);

            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<AdresseEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new AdresseDbService(ctx, auth);
            if (vertrag.Wohnung.Adresse == null)
            {
                throw new Exception("Adresse not found");
            }
            var id = vertrag.Wohnung.Adresse.AdresseId;

            var result = await service.Delete(user, id);

            result.Should().BeOfType<OkResult>();
            ctx.Adressen.Find(id).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new AdresseDbService(ctx, auth);
            var entity = new Adresse("Teststraße", "1", "12345", "Teststadt");
            var entry = new AdresseEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Value.Should().NotBeNull();
            var postedAdresse = Helper.Utils.GetAdresse(entry, ctx);
            postedAdresse.Should().NotBeNull();
        }

        [Fact]
        public async Task PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new AdresseDbService(ctx, auth);
            var entity = new Adresse("Teststraße", "1", "12345", "Teststadt");

            ctx.Adressen.Add(entity);
            ctx.SaveChanges();

            var adresseEntry = new AdresseEntry(entity, new());

            var result = await service.Post(user, adresseEntry);

            result.Result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new AdresseDbService(ctx, auth);
            var entity = new Adresse("Teststraße", "1", "12345", "Teststadt");

            ctx.Adressen.Add(entity);
            ctx.SaveChanges();

            var entry = new AdresseEntry(entity, new());
            entry.Hausnummer = "2";

            var result = await service.Put(user, entity.AdresseId, entry);

            result.Value.Should().NotBeNull();
            var updatedEntity = ctx.Adressen.Find(entity.AdresseId);
            if (updatedEntity == null)
            {
                throw new Exception("Adresse not found");
            }
            updatedEntity.Hausnummer.Should().Be("2");
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new AdresseDbService(ctx, auth);
            var entity = new Adresse("Teststraße", "1", "12345", "Teststadt");
            var entry = new AdresseEntry(entity, new());
            entry.Hausnummer = "2";

            ctx.Adressen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.AdresseId + 22220, entry);

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
