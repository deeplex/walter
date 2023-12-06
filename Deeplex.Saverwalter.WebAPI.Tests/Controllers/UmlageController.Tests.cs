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
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlageControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<UmlageController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlageDbService(ctx, auth);
            var controller = new UmlageController(logger, dbService);
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
            var logger = A.Fake<ILogger<UmlageController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlageDbService(ctx, auth);
            var controller = new UmlageController(logger, dbService);

            var typ = new Umlagetyp("Dachrinnenreinigung");
            ctx.Umlagetypen.Add(typ);
            ctx.SaveChanges();

            var entity = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = typ
            };
            var entry = new UmlageEntry(entity);

            var result = await controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlageController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlageDbService(ctx, auth);
            var controller = new UmlageController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Umlage is null");
            }

            var result = await controller.Get(entity.UmlageId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlageController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlageDbService(ctx, auth);
            var controller = new UmlageController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Umlage is null");
            }
            var entry = new UmlageEntry(entity);
            entry.Beschreibung = "Test";

            var result = await controller.Put(entity.UmlageId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Beschreibung.Should().Be("Test");
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlageController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlageDbService(ctx, auth);
            var controller = new UmlageController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First();
            if (entity == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = entity.UmlageId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagen.Find(id).Should().BeNull();

        }
    }
}
