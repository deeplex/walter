using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ErhaltungsaufwednungDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Get(user, entity.ErhaltungsaufwendungId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<ErhaltungsaufwendungEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Delete(user, entity.ErhaltungsaufwendungId);

            result.Should().BeOfType<OkResult>();
            ctx.Erhaltungsaufwendungen.Find(entity.ErhaltungsaufwendungId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            var entry = new ErhaltungsaufwendungEntry(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();
            var entry = new ErhaltungsaufwendungEntry(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);

            var aussteller = new Kontakt("TestPerson", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };

            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var entry = new ErhaltungsaufwendungEntry(entity);
            entry.Betrag = 2000;

            var result = await service.Put(user, entity.ErhaltungsaufwendungId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Erhaltungsaufwendungen.Find(entity.ErhaltungsaufwendungId);
            if (updatedEntity == null)
            {
                throw new Exception("Erhaltungsaufwendung not found");
            }
            updatedEntity.Betrag.Should().Be(2000);
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new ErhaltungsaufwendungDbService(ctx, auth);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var aussteller = new Kontakt("TestPerson", Rechtsform.ag);
            ctx.Kontakte.Add(aussteller);
            ctx.SaveChanges();

            var entity = new Erhaltungsaufwendung(
                1000, "TestAufwendung", new DateOnly(2021, 1, 1))
            {
                Aussteller = aussteller,
                Wohnung = vertrag.Wohnung
            };
            var entry = new ErhaltungsaufwendungEntry(entity);
            entry.Betrag = 2000;

            ctx.Erhaltungsaufwendungen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.ErhaltungsaufwendungId + 1, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
