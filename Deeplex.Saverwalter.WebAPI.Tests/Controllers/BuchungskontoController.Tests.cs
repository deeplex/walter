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
using Deeplex.Saverwalter.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.BuchungskontoController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BuchungskontoControllerTests
    {
        private static BuchungskontoController WithUser(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            var controller = new BuchungskontoController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
            return controller;
        }

        private static ClaimsPrincipal Principal(UserAccount user) =>
            new(new ClaimsIdentity(user.AssembleClaims(), "mock"));

        /// <summary>
        /// Zwei Wohnungen mit je 7 Konten (2 Wohnung + 5 Vertrag). Der Verwalter verwaltet
        /// nur die erste. Liefert den Verwalter, einen Konto-Id der ersten und einen der
        /// zweiten (nicht verwalteten) Wohnung zurück.
        /// </summary>
        private static (UserAccount manager, int managedKontoId, int foreignKontoId) Seed(SaverwalterContext ctx)
        {
            var manager = new UserAccount { Username = "m", Name = "m", Role = UserRole.User };
            ctx.UserAccounts.Add(manager);

            var v1 = TestUtils.FillVertragWithSomeData(ctx, 0);
            var v2 = TestUtils.FillVertragWithSomeData(ctx, 0);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Vollmacht) { UserAccount = manager, Wohnung = v1.Wohnung });
            ctx.SaveChanges();

            return (manager, v1.MietBuchungskonto.BuchungskontoId, v2.MietBuchungskonto.BuchungskontoId);
        }

        [Fact]
        public async Task GetAll_returns_only_managed_konten_for_verwalter()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = (await controller.GetAll()).Result as OkObjectResult;
            var konten = (result!.Value as IEnumerable<BuchungskontoEntry>)!.ToList();

            konten.Should().HaveCount(7);
        }

        [Fact]
        public async Task GetAll_returns_all_konten_for_admin()
        {
            var ctx = TestUtils.GetContext();
            Seed(ctx);
            var admin = new UserAccount { Username = "a", Name = "a", Role = UserRole.Admin };
            var controller = WithUser(ctx, Principal(admin));

            var result = (await controller.GetAll()).Result as OkObjectResult;
            var konten = (result!.Value as IEnumerable<BuchungskontoEntry>)!.ToList();

            konten.Should().HaveCount(14);
        }

        [Fact]
        public async Task Get_managed_konto_succeeds()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managedKontoId, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Get(managedKontoId);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Get_foreign_konto_is_forbidden()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, foreignKontoId) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Get(foreignKontoId);

            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Get_missing_konto_is_not_found()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Get(999999);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Put_foreign_konto_is_forbidden()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, foreignKontoId) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Put(foreignKontoId, new BuchungskontoUpdateEntry { Bezeichnung = "x" });

            result.Result.Should().BeOfType<ForbidResult>();
        }
    }
}
