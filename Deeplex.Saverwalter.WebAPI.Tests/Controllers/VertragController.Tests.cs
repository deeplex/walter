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
using System.Security.Claims;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService);
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();

            var result = await controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService);

            var besitzer = new Kontakt("Herr Test", Rechtsform.gmbh);
            ctx.Kontakte.Add(besitzer);

            var wohnung = new Wohnung("Test", 100, 100, 1)
            {
                Besitzer = besitzer
            };
            ctx.Wohnungen.Add(wohnung);
            ctx.SaveChanges();

            var entity = new Vertrag()
            {
                Ansprechpartner = wohnung.Besitzer,
                Wohnung = wohnung
            };
            var entry = new VertragEntry(entity);

            var result = await controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService);

            var result = await controller.Get(entity.VertragId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService);

            var entry = new VertragEntry(entity);
            entry.Ende = new DateOnly(2021, 12, 31);

            var result = await controller.Put(entity.VertragId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Ende.Should().Be(new DateOnly(2021, 12, 31));
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var entity = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragDbService(ctx, auth);
            var controller = new VertragController(logger, dbService);

            var id = entity.VertragId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Vertraege.Find(id).Should().BeNull();

        }
    }
}
