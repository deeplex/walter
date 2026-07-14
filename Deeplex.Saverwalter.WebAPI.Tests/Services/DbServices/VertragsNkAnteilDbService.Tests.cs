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
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragsNkAnteilController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    /// <summary>
    /// Deckt Anlegen, Auflisten und Löschen von NK-Sonderforderungen über den
    /// DbService ab — insbesondere, dass die Identifikation strukturell über das
    /// NkSonderVerrechnungsKonto läuft (kein Beschreibungs-Marker mehr) und eine
    /// verteilte BK-Rechnung NICHT als Sonderforderung gilt.
    /// </summary>
    public class VertragsNkAnteilDbServiceTests
    {
        private static (VertragsNkAnteilDbService service, ClaimsPrincipal user, Vertrag vertrag, Umlage umlage)
            Setup(SaverwalterContext ctx)
        {
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

            var service = new VertragsNkAnteilDbService(ctx, auth, new NkAnteilBuchungsService(ctx));
            return (service, user, vertrag, umlage);
        }

        private static VertragsNkAnteilEntry NewEntry(Vertrag vertrag, Umlage umlage, decimal betrag) => new()
        {
            Betrag = betrag,
            Datum = new DateOnly(2024, 12, 31),
            BetreffendesJahr = 2024,
            Vertrag = new SelectionEntry(vertrag.VertragId, "Vertrag"),
            Umlage = new SelectionEntry(umlage.UmlageId, "Umlage"),
        };

        [Fact]
        public async Task Post_BuchtAufSonderkontoUndListetAuf()
        {
            var ctx = TestUtils.GetContext();
            var (service, user, vertrag, umlage) = Setup(ctx);

            var posted = await service.Post(user, NewEntry(vertrag, umlage, 120m));

            posted.Should().NotBeNull();
            // Das Sonderkonto wird bei Bedarf angelegt und trägt die Haben-Seite.
            ctx.Umlagen.Single(u => u.UmlageId == umlage.UmlageId)
                .NkSonderVerrechnungsKonto.Should().NotBeNull();

            var liste = await service.GetList(user, vertragId: null, umlageId: null);
            liste.Should().ContainSingle();
            liste.Single().Betrag.Should().Be(120m);
        }

        [Fact]
        public async Task GetList_IgnoriertVerteilteBkRechnung()
        {
            var ctx = TestUtils.GetContext();
            var (service, user, vertrag, umlage) = Setup(ctx);

            // Eine verteilte BK-Rechnung: Haben auf dem NkVerrechnungsKonto, Soll-Anteil
            // auf dem NkBuchungskonto des Vertrags — sieht wie eine Sonderforderung aus,
            // ist aber keine und darf hier NICHT auftauchen.
            var bk = new Buchungssatz(new DateOnly(2024, 6, 1), "Betriebskosten Wasser 2024") { Buchungsjahr = 2024 };
            bk.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 300m) { Buchungssatz = bk, Buchungskonto = umlage.NkVerrechnungsKonto });
            bk.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 300m) { Buchungssatz = bk, Buchungskonto = vertrag.NkBuchungskonto });
            ctx.Buchungssaetze.Add(bk);
            ctx.SaveChanges();

            // Zusätzlich eine echte Sonderforderung.
            await service.Post(user, NewEntry(vertrag, umlage, 80m));

            var liste = await service.GetList(user, vertragId: null, umlageId: null);
            liste.Should().ContainSingle("nur die Sonderforderung, nicht die verteilte BK-Rechnung");
            liste.Single().Betrag.Should().Be(80m);
        }

        [Fact]
        public async Task Delete_EntferntSonderforderung()
        {
            var ctx = TestUtils.GetContext();
            var (service, user, vertrag, umlage) = Setup(ctx);
            var posted = await service.Post(user, NewEntry(vertrag, umlage, 60m));

            var result = await service.Delete(user, posted.Value!.Id);

            result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkResult>();
            (await service.GetList(user, vertragId: null, umlageId: null)).Should().BeEmpty();
        }

        [Fact]
        public async Task Delete_LehntFremdenSatzAb()
        {
            var ctx = TestUtils.GetContext();
            var (service, user, vertrag, umlage) = Setup(ctx);

            // Verteilte BK-Rechnung (keine Sonderforderung) → Delete muss NotFound liefern.
            var bk = new Buchungssatz(new DateOnly(2024, 6, 1), "Betriebskosten Wasser 2024") { Buchungsjahr = 2024 };
            bk.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 300m) { Buchungssatz = bk, Buchungskonto = umlage.NkVerrechnungsKonto });
            bk.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 300m) { Buchungssatz = bk, Buchungskonto = vertrag.NkBuchungskonto });
            ctx.Buchungssaetze.Add(bk);
            ctx.SaveChanges();

            var result = await service.Delete(user, bk.BuchungssatzId);

            result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
        }
    }
}
