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
            var controller = new AdresseController(logger, dbService);

            var result = await controller.Get();

            result.Should().BeOfType<OkObjectResult>();
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
            var controller = new AdresseController(logger, dbService);

            var entity = new Adresse("Teststrasse", "1", "12345", "Teststadt");
            var entry = new AdresseEntry(entity);

            var result = await controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
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
            var controller = new AdresseController(logger, dbService);

            if (vertrag.Wohnung.Adresse == null)
            {
                throw new NullReferenceException("Adresse is null");
            }

            var result = await controller.Get(vertrag.Wohnung.Adresse.AdresseId);

            result.Should().BeOfType<OkObjectResult>();
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
            var controller = new AdresseController(logger, dbService);

            if (vertrag.Wohnung.Adresse == null)
            {
                throw new NullReferenceException("Adresse is null");
            }
            var entry = new AdresseEntry(vertrag.Wohnung.Adresse);
            entry.Hausnummer = "2";

            var result = await controller.Put(vertrag.Wohnung.Adresse.AdresseId, entry);

            result.Should().BeOfType<OkObjectResult>();
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
            var controller = new AdresseController(logger, dbService);

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
