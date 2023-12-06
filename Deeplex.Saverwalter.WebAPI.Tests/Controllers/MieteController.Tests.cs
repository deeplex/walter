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
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MieteControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService);
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
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = new Miete(new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1), 1000)
            {
                Vertrag = vertrag
            };
            var entry = new MieteEntryBase(entity);

            var result = await controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService);

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Miete is null");
            }

            var result = await controller.Get(vertrag.Mieten.First().MieteId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService);

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Miete is null");
            }
            var entry = new MieteEntryBase(vertrag.Mieten.First());
            entry.Betrag = 2000;

            var result = await controller.Put(vertrag.Mieten.First().MieteId, entry);

            result.Should().BeOfType<OkObjectResult>();
            vertrag.Mieten.First().Betrag.Should().Be(2000);
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, 1000));
            ctx.SaveChanges();
            var logger = A.Fake<ILogger<MieteController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new MieteDbService(ctx, auth);
            var controller = new MieteController(logger, dbService);

            if (vertrag.Mieten.First() == null)
            {
                throw new NullReferenceException("Entity is null");
            }
            var id = vertrag.Mieten.First().MieteId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Mieten.Find(id).Should().BeNull();

        }
    }
}
