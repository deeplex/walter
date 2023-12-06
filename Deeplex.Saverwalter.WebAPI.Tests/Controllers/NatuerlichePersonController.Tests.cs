using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class NatuerlichePersonControllerTests
    {
        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new KontaktDbService(ctx, auth);
            var controller = new KontaktController(logger, dbService);

            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
            var entry = new KontaktEntry(entity);

            var result = await controller.Post(entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new KontaktDbService(ctx, auth);
            var controller = new KontaktController(logger, dbService);

            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var result = await controller.Get(entity.KontaktId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new KontaktDbService(ctx, auth);
            var controller = new KontaktController(logger, dbService);

            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();

            var entry = new KontaktEntry(entity);
            entry.Email = "TestPerson@example.com";

            var result = await controller.Put(entity.KontaktId, entry);

            result.Should().BeOfType<OkObjectResult>();
            entry.Email.Should().Be("TestPerson@example.com");
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new KontaktDbService(ctx, auth);
            var controller = new KontaktController(logger, dbService);

            var entity = new Kontakt("TestPerson", Rechtsform.natuerlich);
            ctx.Kontakte.Add(entity);
            ctx.SaveChanges();
            var id = entity.KontaktId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.Kontakte.Find(id).Should().BeNull();

        }
    }
}
