using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests.Services.PermissionHandling
{
    public class BetriebskostenrechnungAuthTests
    {
        [Theory]
        [InlineData(nameof(Operations.SubCreate), VerwalterRolle.Eigentuemer, false)]
        [InlineData(nameof(Operations.SubCreate), VerwalterRolle.Vollmacht, false)]
        [InlineData(nameof(Operations.SubCreate), VerwalterRolle.Keine, false)]
        [InlineData(nameof(Operations.SubCreate), null, false)]
        [InlineData(nameof(Operations.Delete), VerwalterRolle.Eigentuemer, true)]
        [InlineData(nameof(Operations.Delete), VerwalterRolle.Vollmacht, true)]
        [InlineData(nameof(Operations.Delete), VerwalterRolle.Keine, false)]
        [InlineData(nameof(Operations.Delete), null, false)]
        [InlineData(nameof(Operations.Update), VerwalterRolle.Eigentuemer, true)]
        [InlineData(nameof(Operations.Update), VerwalterRolle.Vollmacht, true)]
        [InlineData(nameof(Operations.Update), VerwalterRolle.Keine, false)]
        [InlineData(nameof(Operations.Update), null, false)]
        [InlineData(nameof(Operations.Read), VerwalterRolle.Eigentuemer, true)]
        [InlineData(nameof(Operations.Read), VerwalterRolle.Vollmacht, true)]
        [InlineData(nameof(Operations.Read), VerwalterRolle.Keine, true)]
        [InlineData(nameof(Operations.Read), null, false)]
        public async Task HandleAsync(string requirementName, VerwalterRolle? rolle, bool success)
        {
            // Arrange
            var ctx = TestUtils.GetContext();
            var user = new UserAccount()
            {
                Username = "test",
                Name = "test",
                Role = UserRole.User
            };
            ctx.UserAccounts.Add(user);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(user.AssembleClaims(), "mock"));
            var requirement = new OperationAuthorizationRequirement()
            {
                Name = requirementName
            };
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = vertrag.Wohnung.Umlagen.First().Betriebskostenrechnungen.First();

            if (rolle is VerwalterRolle r)
            {
                var verwalter = new Verwalter(r)
                {
                    UserAccount = user,
                    Wohnung = vertrag.Wohnung
                };
                ctx.VerwalterSet.Add(verwalter);
            };

            ctx.SaveChanges();

            var authContext = new AuthorizationHandlerContext([requirement], claimsPrincipal, entity);
            var permissionHandler = new BetriebskostenrechnungPermissionHandler();

            // Act
            await permissionHandler.HandleAsync(authContext);

            // Assert
            authContext.HasSucceeded.Should().Be(success);
        }
    }
}
