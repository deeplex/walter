using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BetriebskostenrechnungControllerTests : IDisposable
    {
        public SaverwalterContext ctx;
        public BetriebskostenrechnungControllerTests()
        {
            ctx = TestUtils.GetContext();
        }

        public void Dispose()
        {
            ctx.Dispose();
        }

        [Fact]
        public async Task Get()
        {
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new BetriebskostenrechnungDbService(ctx, auth);
            var controller = new BetriebskostenrechnungController(logger, dbService);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();

            var result = await controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Post()
        {
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new BetriebskostenrechnungDbService(ctx, auth);
            var controller = new BetriebskostenrechnungController(logger, dbService);

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
            var entry = new BetriebskostenrechnungEntry(entity);

            var result = await controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetId()
        {
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new BetriebskostenrechnungDbService(ctx, auth);
            var controller = new BetriebskostenrechnungController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Betriebskostenrechnungen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Betriebskostenrechnung is null");
            }

            var result = await controller.Get(entity.BetriebskostenrechnungId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Put()
        {
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new BetriebskostenrechnungDbService(ctx, auth);
            var controller = new BetriebskostenrechnungController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Betriebskostenrechnungen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Betriebskostenrechnung is null");
            }
            var entry = new BetriebskostenrechnungEntry(entity);
            entry.Betrag = 2000;

            var result = await controller.Put(entity.BetriebskostenrechnungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Betrag.Should().Be(2000);
        }

        [Fact]
        public async Task Delete()
        {
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<BetriebskostenrechnungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new BetriebskostenrechnungDbService(ctx, auth);
            var controller = new BetriebskostenrechnungController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Betriebskostenrechnungen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = entity.BetriebskostenrechnungId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Betriebskostenrechnungen.Find(id).Should().BeNull();
        }
    }
}
