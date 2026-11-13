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
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BetriebskostenrechnungDbServiceTests
    {
        private static (SaverwalterContext ctx, BetriebskostenrechnungDbService service, ClaimsPrincipal user, Vertrag vertrag, Umlage umlage, Buchungssatz rechnung) Setup()
        {
            var ctx = TestUtils.GetContext();
            var user = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => user.IsInRole("Admin")).Returns(true);
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));

            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Wasser"),
                Wohnungen = [vertrag.Wohnung],
                NkVerrechnungsKonto = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1200", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(umlage);
            ctx.SaveChanges();

            var buchungsService = new BetriebskostenrechnungBuchungsService(ctx);
            var rechnung = buchungsService.BucheRechnungAsync(umlage, 300m, new DateOnly(2024, 6, 1), 2024, null).Result;
            var service = new BetriebskostenrechnungDbService(ctx, auth, buchungsService, new BuchungssatzSchutzService(ctx));
            return (ctx, service, user, vertrag, umlage, rechnung);
        }

        private static void Abrechnen(SaverwalterContext ctx, Vertrag vertrag, int jahr)
        {
            var satz = new Buchungssatz(new DateOnly(jahr, 12, 31), $"BK-Abrechnung {jahr}") { Buchungsjahr = jahr };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 100m) { Buchungssatz = satz, Buchungskonto = vertrag.NkBuchungskonto });
            ctx.Buchungssaetze.Add(satz);
            ctx.Abrechnungsresultate.Add(new Abrechnungsresultat { Vertrag = vertrag, Buchungssatz = satz });
            ctx.SaveChanges();
        }

        private static BetriebskostenrechnungEntry Entry(Umlage umlage, decimal betrag, int jahr, string? notiz = null) => new()
        {
            Betrag = betrag,
            Datum = new DateOnly(2024, 6, 1),
            BetreffendesJahr = jahr,
            Umlage = new SelectionEntry(umlage.UmlageId, "Wasser"),
            Notiz = notiz,
        };

        [Fact]
        public async Task DeleteEinerFreienRechnungEntferntSie()
        {
            var (ctx, service, user, _, _, rechnung) = Setup();

            (await service.Delete(user, rechnung.BuchungssatzId)).Should().BeOfType<OkResult>();
            ctx.Buchungssaetze.Any(s => s.BuchungssatzId == rechnung.BuchungssatzId).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteNachAbrechnungIstGesperrt()
        {
            var (ctx, service, user, vertrag, _, rechnung) = Setup();
            Abrechnen(ctx, vertrag, 2024);

            (await service.Delete(user, rechnung.BuchungssatzId)).Should().BeOfType<ConflictObjectResult>();
            ctx.Buchungssaetze.Any(s => s.BuchungssatzId == rechnung.BuchungssatzId).Should().BeTrue();
        }

        [Fact]
        public async Task BetragAendernNachAbrechnungIstGesperrt()
        {
            var (ctx, service, user, vertrag, umlage, rechnung) = Setup();
            Abrechnen(ctx, vertrag, 2024);

            var result = await service.Put(user, rechnung.BuchungssatzId, Entry(umlage, 450m, 2024));

            result.Result.Should().BeOfType<ConflictObjectResult>();
            rechnung.Buchungszeilen.Single().Betrag.Should().Be(300m);
        }

        [Fact]
        public async Task JahrInAbgerechnetesJahrVerschiebenIstGesperrt()
        {
            var (ctx, service, user, vertrag, umlage, rechnung) = Setup();
            Abrechnen(ctx, vertrag, 2023);

            var result = await service.Put(user, rechnung.BuchungssatzId, Entry(umlage, 300m, 2023));

            result.Result.Should().BeOfType<ConflictObjectResult>();
            rechnung.Buchungsjahr.Should().Be(2024);
        }

        [Fact]
        public async Task NotizAendernBleibtNachAbrechnungMoeglich()
        {
            var (ctx, service, user, vertrag, umlage, rechnung) = Setup();
            Abrechnen(ctx, vertrag, 2024);

            var result = await service.Put(user, rechnung.BuchungssatzId, Entry(umlage, 300m, 2024, "Rechnung liegt vor"));

            result.Value.Should().NotBeNull();
            rechnung.Notiz.Should().Be("Rechnung liegt vor");
        }
    }
}
