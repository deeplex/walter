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
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.BuchungssaetzeController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BuchungssaetzeControllerTests
    {
        private static BuchungssaetzeController WithUser(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            var controller = new BuchungssaetzeController(new StornoBuchungsService(ctx), ctx, null!)
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

        private static Buchungssatz AddSatz(SaverwalterContext ctx, Vertrag vertrag, string beschreibung, decimal betrag)
        {
            var satz = new Buchungssatz(new DateOnly(2026, 5, 1), beschreibung);
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.MietBuchungskonto
            });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.ZahlungsKonto
            });
            ctx.Buchungssaetze.Add(satz);
            return satz;
        }

        /// <summary>
        /// Zwei Wohnungen mit Verträgen und je einem Buchungssatz. Der Verwalter
        /// verwaltet nur die erste Wohnung.
        /// </summary>
        private static (UserAccount manager, Buchungssatz managedSatz, Buchungssatz foreignSatz, Vertrag managedVertrag) Seed(SaverwalterContext ctx)
        {
            var manager = new UserAccount { Username = "m", Name = "m", Role = UserRole.User };
            ctx.UserAccounts.Add(manager);

            var v1 = TestUtils.FillVertragWithSomeData(ctx, 0);
            var v2 = TestUtils.FillVertragWithSomeData(ctx, 0);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Vollmacht) { UserAccount = manager, Wohnung = v1.Wohnung });

            var managedSatz = AddSatz(ctx, v1, "Mietzahlung Mai", 500m);
            var foreignSatz = AddSatz(ctx, v2, "Fremde Buchung", 700m);
            ctx.SaveChanges();

            return (manager, managedSatz, foreignSatz, v1);
        }

        [Fact]
        public async Task GetList_returns_only_saetze_on_managed_konten()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managedSatz, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = (await controller.GetList(new PagedQuery(), kontoId: null)).Result as OkObjectResult;
            var paged = result!.Value as PagedResult<BuchungssatzEntryBase>;

            paged!.TotalCount.Should().Be(1);
            paged.Items.Single().Id.Should().Be(managedSatz.BuchungssatzId);
        }

        [Fact]
        public async Task GetList_returns_all_saetze_for_admin()
        {
            var ctx = TestUtils.GetContext();
            Seed(ctx);
            var admin = new UserAccount { Username = "a", Name = "a", Role = UserRole.Admin };
            var controller = WithUser(ctx, Principal(admin));

            var result = (await controller.GetList(new PagedQuery(), kontoId: null)).Result as OkObjectResult;
            var paged = result!.Value as PagedResult<BuchungssatzEntryBase>;

            paged!.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetList_filters_by_konto()
        {
            var ctx = TestUtils.GetContext();
            var (_, managedSatz, _, vertrag) = Seed(ctx);
            var admin = new UserAccount { Username = "a", Name = "a", Role = UserRole.Admin };
            var controller = WithUser(ctx, Principal(admin));

            var result = (await controller.GetList(
                new PagedQuery(), vertrag.MietBuchungskonto.BuchungskontoId)).Result as OkObjectResult;
            var paged = result!.Value as PagedResult<BuchungssatzEntryBase>;

            paged!.TotalCount.Should().Be(1);
            var entry = paged.Items.Single();
            entry.Id.Should().Be(managedSatz.BuchungssatzId);
            // Kontoblatt: Soll/Haben aus Sicht des gefilterten Kontos
            entry.KontoSoll.Should().Be(500m);
            entry.KontoHaben.Should().BeNull();
        }

        [Fact]
        public async Task Get_managed_satz_includes_zeilen_und_verknuepfungen()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managedSatz, _, vertrag) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = (await controller.Get(managedSatz.BuchungssatzId)).Result as OkObjectResult;
            var entry = result!.Value as BuchungssatzEntry;

            entry!.Zeilen.Should().HaveCount(2);
            entry.Betrag.Should().Be(500m);
            entry.Verknuepfungen.Should().Contain(v =>
                v.Typ == "Vertrag" && v.Id == vertrag.VertragId.ToString());
        }

        [Fact]
        public async Task Get_foreign_satz_is_forbidden()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, foreignSatz, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Get(foreignSatz.BuchungssatzId);

            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Get_missing_satz_is_not_found()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Get(Guid.NewGuid());

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
