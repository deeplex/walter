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
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class VertragVersionenControllerTests
    {
        [Fact]
        public async Task Post()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<VertragVersionController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragVersionDbService(ctx, auth);
            var controller = new VertragVersionController(logger, dbService);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var entity = new VertragVersion(new DateOnly(2021, 6, 30), 1000, 2)
            {
                Vertrag = vertrag
            };
            var entry = new VertragVersionEntry(entity, new());

            var result = await controller.Post(entry);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetId()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragVersionController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragVersionDbService(ctx, auth);
            var controller = new VertragVersionController(logger, dbService);
            var entity = vertrag.Versionen.First();

            if (entity == null)
            {
                throw new NullReferenceException("Vertrag has no Versionen");
            }

            var result = await controller.Get(entity.VertragVersionId);

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Put()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragVersionController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragVersionDbService(ctx, auth);
            var controller = new VertragVersionController(logger, dbService);
            var entity = vertrag.Versionen.First();

            if (entity == null)
            {
                throw new NullReferenceException("Vertrag has no Versionen");
            }

            var entry = new VertragVersionEntry(entity, new());
            entry.Personenzahl = 4;

            var result = await controller.Put(entity.VertragVersionId, entry);

            result.Value.Should().NotBeNull();
            entity.Personenzahl.Should().Be(4);
        }

        [Fact]
        public async Task Delete()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<VertragVersionController>>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(null!, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new VertragVersionDbService(ctx, auth);
            var controller = new VertragVersionController(logger, dbService);
            var entity = vertrag.Versionen.First();

            var id = entity.VertragVersionId;

            var result = await controller.Delete(id);

            result.Should().BeOfType<OkResult>();
            ctx.VertragVersionen.Find(id).Should().BeNull();

        }
    }
}
