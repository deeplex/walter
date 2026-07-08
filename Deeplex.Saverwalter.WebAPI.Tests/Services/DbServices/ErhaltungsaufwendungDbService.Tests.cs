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
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ErhaltungsaufwendungDbServiceTests
    {
        private static (SaverwalterContext ctx, IAuthorizationService auth, ClaimsPrincipal user) Setup()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => user.IsInRole("Admin")).Returns(true);
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            return (ctx, auth, user);
        }

        private static ErhaltungsaufwendungDbService MakeService(SaverwalterContext ctx, IAuthorizationService auth)
            => new(ctx, auth, new ErhaltungsaufwendungBuchungsService(ctx));

        [Fact]
        public async Task PostErzeugtBuchungssatzUndGibtEintragZurueck()
        {
            var (ctx, auth, user) = Setup();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            await ctx.SaveChangesAsync();
            var satzCountBefore = ctx.Buchungssaetze.Count();

            var service = MakeService(ctx, auth);
            var entry = new ErhaltungsaufwendungEntry
            {
                Betrag = 750m,
                Datum = new DateOnly(2024, 4, 1),
                Bezeichnung = "Dachreparatur",
                Wohnung = new SelectionEntry(vertrag.Wohnung.WohnungId, vertrag.Wohnung.Bezeichnung),
                Aussteller = new SelectionEntry(aussteller.KontaktId, aussteller.Bezeichnung),
            };

            var result = await service.Post(user, entry);

            result.Value.Should().NotBeNull();
            result.Value!.Betrag.Should().Be(750m);
            result.Value.Bezeichnung.Should().Be("Dachreparatur");
            // Es wurde genau ein Buchungssatz (auf dem AufwandsKonto) ergänzt.
            ctx.Buchungssaetze.Count().Should().Be(satzCountBefore + 1);
        }

        [Fact]
        public async Task PostOhneAusstellerGibtBadRequest()
        {
            var (ctx, auth, user) = Setup();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var service = MakeService(ctx, auth);
            var entry = new ErhaltungsaufwendungEntry
            {
                Betrag = 100m,
                Datum = new DateOnly(2024, 1, 1),
                Wohnung = new SelectionEntry(vertrag.Wohnung.WohnungId, vertrag.Wohnung.Bezeichnung),
                Aussteller = null,
            };

            var result = await service.Post(user, entry);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetListLiefertGebuchteErhaltungsaufwendung()
        {
            var (ctx, auth, user) = Setup();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            await ctx.SaveChangesAsync();

            var service = MakeService(ctx, auth);
            await service.Post(user, new ErhaltungsaufwendungEntry
            {
                Betrag = 200m,
                Datum = new DateOnly(2024, 2, 1),
                Bezeichnung = "Malerarbeiten",
                Wohnung = new SelectionEntry(vertrag.Wohnung.WohnungId, vertrag.Wohnung.Bezeichnung),
                Aussteller = new SelectionEntry(aussteller.KontaktId, aussteller.Bezeichnung),
            });

            var list = await service.GetList(user, new PagedQuery());

            list.Items.Should().ContainSingle();
            list.Items.Single().Betrag.Should().Be(200m);
        }

        [Fact]
        public async Task DeleteEntferntBuchungssatz()
        {
            var (ctx, auth, user) = Setup();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh);
            ctx.Kontakte.Add(aussteller);
            await ctx.SaveChangesAsync();
            var satzCountBefore = ctx.Buchungssaetze.Count();

            var service = MakeService(ctx, auth);
            var posted = await service.Post(user, new ErhaltungsaufwendungEntry
            {
                Betrag = 200m,
                Datum = new DateOnly(2024, 2, 1),
                Bezeichnung = "Malerarbeiten",
                Wohnung = new SelectionEntry(vertrag.Wohnung.WohnungId, vertrag.Wohnung.Bezeichnung),
                Aussteller = new SelectionEntry(aussteller.KontaktId, aussteller.Bezeichnung),
            });

            var result = await service.Delete(user, posted.Value!.Id);

            result.Should().BeOfType<OkResult>();
            // Der EA-Buchungssatz wurde wieder entfernt (Ausgangszustand).
            ctx.Buchungssaetze.Count().Should().Be(satzCountBefore);
        }
    }
}
