using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlagetypDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlagetypDbService(ctx, auth);
            var entity = new Umlagetyp("Hausstrom");

            var umlage = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = entity
            };

            var wohnung = new Wohnung("whatever", 0, 0, 0);

            umlage.Wohnungen.Add(wohnung);

            ctx.Umlagen.Add(umlage);
            ctx.Wohnungen.Add(wohnung);
            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Get(user, entity.UmlagetypId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<UmlagetypEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlagetypDbService(ctx, auth);
            var entity = new Umlagetyp("Hausstrom");
            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Delete(user, entity.UmlagetypId);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagen.Find(entity.UmlagetypId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlagetypDbService(ctx, auth);
            var entity = new Umlagetyp("Hausstrom");
            var entry = new UmlagetypEntry(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task PostFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlagetypDbService(ctx, auth);
            var entity = new Umlagetyp("Hausstrom");

            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();
            var entry = new UmlagetypEntry(entity);

            var result = await service.Post(user, entry);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task PutTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlagetypDbService(ctx, auth);
            var entity = new Umlagetyp("Hausstrom");

            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();

            var entry = new UmlagetypEntry(entity);
            entry.Bezeichnung = "Test";

            var result = await service.Put(user, entity.UmlagetypId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Umlagetypen.Find(entity.UmlagetypId);
            if (updatedEntity == null)
            {
                throw new Exception("Umlagetyp not found");
            }
            updatedEntity.Bezeichnung.Should().Be("Test");
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlagetypDbService(ctx, auth);
            var entity = new Umlagetyp("Hausstrom");
            var entry = new UmlagetypEntry(entity);
            entry.Bezeichnung = "Test";

            ctx.Umlagetypen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.UmlagetypId + 2221, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
