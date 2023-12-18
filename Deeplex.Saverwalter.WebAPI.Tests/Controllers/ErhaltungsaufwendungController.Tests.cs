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
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ErhaltungsaufwendungControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ErhaltungsaufwendungDbService(ctx, auth);
            var controller = new ErhaltungsaufwendungController(logger, dbService, A.Fake<HttpClient>());
            controller.ControllerContext = A.Fake<ControllerContext>();
            controller.ControllerContext.HttpContext = A.Fake<HttpContext>();
            controller.ControllerContext.HttpContext.User = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => controller.ControllerContext.HttpContext.User.IsInRole("Admin")).Returns(true);

            var result = await controller.Get();

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ErhaltungsaufwendungDbService(ctx, auth);
            var controller = new ErhaltungsaufwendungController(logger, dbService, A.Fake<HttpClient>());

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(1000, "Test", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            var entry = new ErhaltungsaufwendungEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ErhaltungsaufwendungDbService(ctx, auth);
            var controller = new ErhaltungsaufwendungController(logger, dbService, A.Fake<HttpClient>());

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(1000, "Test", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = await controller.Get(entity.ErhaltungsaufwendungId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ErhaltungsaufwendungDbService(ctx, auth);
            var controller = new ErhaltungsaufwendungController(logger, dbService, A.Fake<HttpClient>());

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(1000, "Test", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var entry = new ErhaltungsaufwendungEntry(entity, new());
            entry.Betrag = 2000;

            var result = await controller.Put(entity.ErhaltungsaufwendungId, entry);

            result.Value.Should().NotBeNull();
            entry.Betrag.Should().Be(2000);
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<ErhaltungsaufwendungController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new ErhaltungsaufwendungDbService(ctx, auth);
            var controller = new ErhaltungsaufwendungController(logger, dbService, A.Fake<HttpClient>());

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(1000, "Test", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();
            var id = entity.ErhaltungsaufwendungId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Erhaltungsaufwendungen.Find(id).Should().BeNull();

        }
    }
}
