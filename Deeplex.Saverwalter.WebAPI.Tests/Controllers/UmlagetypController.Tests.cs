using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlagetypControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService);

            var result = await controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService);

            var entity = new Umlagetyp("Hausstrom");
            var entry = new UmlagetypEntry(entity);

            var result = await controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Umlagetyp is null");
            }

            var result = await controller.Get(entity.UmlagetypId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Umlagetyp is null");
            }
            var entry = new UmlagetypEntry(entity);
            entry.Bezeichnung = "Test";

            var result = await controller.Put(entity.UmlagetypId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entity.Bezeichnung.Should().Be("Test");
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<UmlagetypController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new UmlagetypDbService(ctx, auth);
            var controller = new UmlagetypController(logger, dbService);

            var entity = vertrag.Wohnung.Umlagen.First().Typ;
            if (entity == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = entity.UmlagetypId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagetypen.Find(id).Should().BeNull();

        }
    }
}
