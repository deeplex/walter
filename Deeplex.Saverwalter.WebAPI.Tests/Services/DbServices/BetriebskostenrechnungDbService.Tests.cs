using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BetriebskostenrechnungDbServiceTests : IDisposable
    {
        public SaverwalterContext ctx;
        public BetriebskostenrechnungDbServiceTests()
        {
            ctx = TestUtils.GetContext();
        }

        public void Dispose()
        {
            ctx.Dispose();
        }

        [Fact]
        public async Task GetTest()
        {
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new BetriebskostenrechnungDbService(ctx, auth);

            var result = await service.Get(
                user,
                vertrag.Wohnung.Umlagen.First().Betriebskostenrechnungen.First().BetriebskostenrechnungId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<BetriebskostenrechnungEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new BetriebskostenrechnungDbService(ctx, auth);
            var id = vertrag.
                Wohnung.
                Umlagen.First().
                Betriebskostenrechnungen.First().
                BetriebskostenrechnungId;

            var result = await service.Delete(user, id);

            result.Should().BeOfType<OkResult>();
            ctx.Betriebskostenrechnungen.Find(id).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new BetriebskostenrechnungDbService(ctx, auth);
            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung")
            };
            ctx.Umlagen.Add(umlage);
            ctx.SaveChanges();
            var entity = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };
            var entry = new BetriebskostenrechnungEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task PostFailedTest()
        {
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new BetriebskostenrechnungDbService(ctx, auth);
            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung")
            };
            var entity = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };

            ctx.Betriebskostenrechnungen.Add(entity);
            ctx.SaveChanges();

            var entry = new BetriebskostenrechnungEntry(entity, new());

            var result = await service.Post(user, entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new BetriebskostenrechnungDbService(ctx, auth);
            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung")
            };
            var entity = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };
            ctx.Betriebskostenrechnungen.Add(entity);
            ctx.SaveChanges();
            var entry = new BetriebskostenrechnungEntry(entity, new());
            entry.Betrag = 2000;

            var result = await service.Put(user, entity.BetriebskostenrechnungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Betriebskostenrechnungen.Find(entity.BetriebskostenrechnungId);
            if (updatedEntity == null)
            {
                throw new Exception("Betriebskostenrechnung not found");
            }
            updatedEntity.Betrag.Should().Be(2000);
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new BetriebskostenrechnungDbService(ctx, auth);
            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung")
            };
            var entity = new Betriebskostenrechnung(1000, new DateOnly(2021, 1, 1), 2021)
            {
                Umlage = umlage
            };
            var entry = new BetriebskostenrechnungEntry(entity, new());
            ctx.Betriebskostenrechnungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.BetriebskostenrechnungId + 312, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
