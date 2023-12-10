using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UmlageDbServiceTests
    {
        [Fact]
        public async Task GetTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlageDbService(ctx, auth);
            var entity = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung")
            };

            var wohnung = new Wohnung("whatever", 0, 0, 0);
            entity.Wohnungen.Add(wohnung);

            ctx.Wohnungen.Add(wohnung);
            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Get(user, entity.UmlageId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeOfType<UmlageEntry>();
        }

        [Fact]
        public async Task DeleteTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlageDbService(ctx, auth);
            var entity = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung")
            };
            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Delete(user, entity.UmlageId);

            result.Should().BeOfType<OkResult>();
            ctx.Umlagen.Find(entity.UmlageId).Should().BeNull();
        }

        [Fact]
        public async Task PostTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlageDbService(ctx, auth);
            var typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung");
            ctx.Umlagetypen.Add(typ);
            ctx.SaveChanges();
            var entity = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = typ
            };
            var entry = new UmlageEntry(entity);

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
            var service = new UmlageDbService(ctx, auth);
            var entity = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung")
            };

            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();
            var entry = new UmlageEntry(entity);

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
            var service = new UmlageDbService(ctx, auth);
            var entity = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung")
            };

            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();

            var entry = new UmlageEntry(entity);
            entry.Beschreibung = "Test";

            var result = await service.Put(user, entity.UmlageId, entry);

            result.Should().BeOfType<OkObjectResult>();
            var updatedEntity = ctx.Umlagen.Find(entity.UmlageId);
            if (updatedEntity == null)
            {
                throw new Exception("Umlage not found");
            }
            updatedEntity.Beschreibung.Should().Be("Test");
        }

        [Fact]
        public async Task PutFailedTest()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var service = new UmlageDbService(ctx, auth);
            var entity = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung")
            };
            var entry = new UmlageEntry(entity);
            entry.Beschreibung = "Test";

            ctx.Umlagen.Add(entity);
            ctx.SaveChanges();

            var result = await service.Put(user, entity.UmlageId + 11, entry);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
