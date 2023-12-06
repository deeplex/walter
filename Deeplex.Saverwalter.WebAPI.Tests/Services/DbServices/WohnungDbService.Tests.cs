using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class WohnungDbServiceTests
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
            var service = new WohnungDbService(ctx, auth);

            var result = await service.Get(user, vertrag.Wohnung.WohnungId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<WohnungEntry>();
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
            var service = new WohnungDbService(ctx, auth);

            var id = vertrag.Wohnung.WohnungId;
            var result = await service.Delete(user, id);

            result.Should().BeOfType<OkResult>();
            ctx.Wohnungen.Find(id).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);
            ctx.SaveChanges();

            var entity = new Wohnung("Test", 100, 100, 1)
            {
                Besitzer = besitzer
            };
            var entry = new WohnungEntry(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var entity = new Wohnung("Test", 100, 100, 1)
            {
                Besitzer = besitzer
            };

            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();

            var entry = new WohnungEntry(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);
            ctx.SaveChanges();

            var entity = new Wohnung("Test", 100, 100, 1)
            {
                Besitzer = besitzer
            };
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();
            var entry = new WohnungEntry(entity);
            entry.Wohnflaeche = 200;

            var result = await service.Put(user, entity.WohnungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Wohnungen.Find(entity.WohnungId);
            if (updatedEntity == null)
            {
                throw new Exception("Wohnung not found");
            }
            updatedEntity.Wohnflaeche.Should().Be(200);
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new WohnungDbService(ctx, auth);
            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung")
            };

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var entity = new Wohnung("Test", 100, 100, 1)
            {
                Besitzer = besitzer
            };
            var entry = new WohnungEntry(entity);
            ctx.Wohnungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.WohnungId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
