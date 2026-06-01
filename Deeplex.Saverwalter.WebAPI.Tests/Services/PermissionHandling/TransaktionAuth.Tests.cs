// Copyright (c) 2023-2026 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests.Services.PermissionHandling
{
    public class TransaktionAuthTests
    {
        /// <summary>
        /// Hängt eine Transaktion an den Vertrag: eine Mietzahlung Soll ZahlungsKonto /
        /// Haben MietBuchungskonto — so wie es der Seed / TransaktionBuchungsService anlegt.
        /// </summary>
        private static Transaktion AddZahlung(SaverwalterContext ctx, Vertrag vertrag)
        {
            var transaktion = new Transaktion
            {
                Zahlungsdatum = new DateOnly(2021, 1, 1),
                Betrag = 500,
                Verwendungszweck = "Miete"
            };
            var satz = new Buchungssatz(new DateOnly(2021, 1, 1), "Mietzahlung")
            {
                Transaktion = transaktion
            };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 500)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.ZahlungsKonto
            });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 500)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.MietBuchungskonto
            });
            ctx.Buchungssaetze.Add(satz);
            ctx.SaveChanges();
            return transaktion;
        }

        private static (SaverwalterContext ctx, UserAccount user, ClaimsPrincipal principal) Setup(
            VerwalterRolle? rolle)
        {
            var ctx = TestUtils.GetContext();
            var user = new UserAccount { Username = "test", Name = "test", Role = UserRole.User };
            ctx.UserAccounts.Add(user);

            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            if (rolle is VerwalterRolle r)
            {
                ctx.VerwalterSet.Add(new Verwalter(r) { UserAccount = user, Wohnung = vertrag.Wohnung });
            }
            ctx.SaveChanges();
            AddZahlung(ctx, vertrag);

            var principal = new ClaimsPrincipal(new ClaimsIdentity(user.AssembleClaims(), "mock"));
            return (ctx, user, principal);
        }

        [Fact]
        public async Task GetQueryable_returns_transaktion_for_managing_verwalter()
        {
            var (ctx, _, principal) = Setup(VerwalterRolle.Vollmacht);

            var result = await TransaktionPermissionHandler.GetQueryable(ctx, principal).ToListAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetQueryable_returns_transaktion_for_read_only_verwalter()
        {
            var (ctx, _, principal) = Setup(VerwalterRolle.Keine);

            var result = await TransaktionPermissionHandler.GetQueryable(ctx, principal).ToListAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetQueryable_hides_transaktion_from_unrelated_user()
        {
            var (ctx, _, principal) = Setup(rolle: null);

            var result = await TransaktionPermissionHandler.GetQueryable(ctx, principal).ToListAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetQueryable_returns_all_for_admin()
        {
            var (ctx, _, _) = Setup(rolle: null);
            var admin = new UserAccount { Username = "a", Name = "a", Role = UserRole.Admin };
            var adminPrincipal = new ClaimsPrincipal(new ClaimsIdentity(admin.AssembleClaims(), "mock"));

            var result = await TransaktionPermissionHandler.GetQueryable(ctx, adminPrincipal).ToListAsync();

            result.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(nameof(Operations.Read), VerwalterRolle.Eigentuemer, true)]
        [InlineData(nameof(Operations.Read), VerwalterRolle.Vollmacht, true)]
        [InlineData(nameof(Operations.Read), VerwalterRolle.Keine, true)]
        [InlineData(nameof(Operations.Read), null, false)]
        [InlineData(nameof(Operations.Update), VerwalterRolle.Eigentuemer, true)]
        [InlineData(nameof(Operations.Update), VerwalterRolle.Vollmacht, true)]
        [InlineData(nameof(Operations.Update), VerwalterRolle.Keine, false)]
        [InlineData(nameof(Operations.Update), null, false)]
        [InlineData(nameof(Operations.Delete), VerwalterRolle.Vollmacht, true)]
        [InlineData(nameof(Operations.Delete), VerwalterRolle.Keine, false)]
        public async Task HandleAsync_single_transaktion(string requirementName, VerwalterRolle? rolle, bool success)
        {
            var (ctx, _, principal) = Setup(rolle);
            var transaktion = await ctx.Transaktionen.SingleAsync();
            var requirement = new OperationAuthorizationRequirement { Name = requirementName };
            var authContext = new AuthorizationHandlerContext([requirement], principal, transaktion);

            var handler = new TransaktionPermissionHandler(ctx);
            await handler.HandleAsync(authContext);

            authContext.HasSucceeded.Should().Be(success);
        }
    }
}
