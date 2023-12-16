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
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AdresseControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<AdresseController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, A.Fake<HttpClient>());
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
            var logger = A.Fake<ILogger<AdresseController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, A.Fake<HttpClient>());

            var entity = new Adresse("Teststrasse", "1", "12345", "Teststadt");
            var entry = new AdresseEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, A.Fake<HttpClient>());

            if (vertrag.Wohnung.Adresse == null)
            {
                throw new NullReferenceException("Adresse is null");
            }

            var result = await controller.Get(vertrag.Wohnung.Adresse.AdresseId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, A.Fake<HttpClient>());

            if (vertrag.Wohnung.Adresse == null)
            {
                throw new NullReferenceException("Adresse is null");
            }
            var entry = new AdresseEntry(vertrag.Wohnung.Adresse, new());
            entry.Hausnummer = "2";

            var result = await controller.Put(vertrag.Wohnung.Adresse.AdresseId, entry);

            result.Value.Should().NotBeNull();
            vertrag.Wohnung.Adresse.Hausnummer.Should().Be("2");
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, A.Fake<HttpClient>());

            if (vertrag.Wohnung.Adresse == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = vertrag.Wohnung.Adresse.AdresseId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Adressen.Find(id).Should().BeNull();

        }
    }
}
