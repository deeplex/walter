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
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class TransaktionDbServiceTests
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

        private static TransaktionDbService MakeService(SaverwalterContext ctx, IAuthorizationService auth)
            => new(ctx, auth, new BuchungssatzSchutzService(ctx), new StornoBuchungsService(ctx));

        private static Transaktion NeueTransaktion() => new()
        {
            Zahlungsdatum = new DateOnly(2024, 1, 5),
            Betrag = 100m,
            Verwendungszweck = "Test",
        };

        [Fact]
        public async Task DeleteFreieTransaktion_EntferntTransaktionUndBuchungssaetze()
        {
            var (ctx, auth, user) = Setup();

            var bank = new Buchungskonto("1000", "Bank", BuchungskontoTyp.Aktiv);
            var ertrag = new Buchungskonto("4000", "Ertrag", BuchungskontoTyp.Ertrag);

            var transaktion = NeueTransaktion();
            var satz = new Buchungssatz(new DateOnly(2024, 1, 5), "Freie Buchung") { Transaktion = transaktion };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 100m) { Buchungssatz = satz, Buchungskonto = bank });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 100m) { Buchungssatz = satz, Buchungskonto = ertrag });

            ctx.Transaktionen.Add(transaktion);
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var service = MakeService(ctx, auth);
            var result = await service.Delete(user, transaktion.TransaktionId);

            result.Should().BeOfType<OkResult>();
            ctx.Transaktionen.Any(t => t.TransaktionId == transaktion.TransaktionId).Should().BeFalse();
            ctx.Buchungssaetze.Any(s => s.BuchungssatzId == satz.BuchungssatzId).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteMitOposVerknuepfung_Blockiert_StornoErlaubt()
        {
            var (ctx, auth, user) = Setup();

            var forderung = new Buchungskonto("1400", "Forderung", BuchungskontoTyp.Aktiv);
            var ertrag = new Buchungskonto("4000", "Ertrag", BuchungskontoTyp.Ertrag);
            var bank = new Buchungskonto("1000", "Bank", BuchungskontoTyp.Aktiv);

            // Sollstellung (eigenständig): Soll Forderung / Haben Ertrag
            var sollstellung = new Buchungssatz(new DateOnly(2024, 1, 1), "Sollstellung");
            var sollZeile = new Buchungszeile(SollHaben.Soll, 100m) { Buchungssatz = sollstellung, Buchungskonto = forderung };
            sollstellung.Buchungszeilen.Add(sollZeile);
            sollstellung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 100m) { Buchungssatz = sollstellung, Buchungskonto = ertrag });

            // Zahlung in der Transaktion: Soll Bank / Haben Forderung — gleicht die Forderung aus
            var transaktion = NeueTransaktion();
            var zahlung = new Buchungssatz(new DateOnly(2024, 1, 5), "Zahlung") { Transaktion = transaktion };
            zahlung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 100m) { Buchungssatz = zahlung, Buchungskonto = bank });
            var habenZeile = new Buchungszeile(SollHaben.Haben, 100m) { Buchungssatz = zahlung, Buchungskonto = forderung };
            zahlung.Buchungszeilen.Add(habenZeile);

            var ausgleich = new OffenerPostenAusgleich { SollZeile = sollZeile, HabenZeile = habenZeile };

            ctx.Transaktionen.Add(transaktion);
            ctx.Buchungssaetze.AddRange(sollstellung, zahlung);
            ctx.OffenePostenAusgleiche.Add(ausgleich);
            await ctx.SaveChangesAsync();

            var service = MakeService(ctx, auth);

            // Löschen ist gesperrt (OPOS-verknüpft) — nichts wird entfernt.
            var del = await service.Delete(user, transaktion.TransaktionId);
            del.Should().BeOfType<ConflictObjectResult>()
                .Which.Value.Should().BeOfType<string>()
                .Which.Should().Contain("stornieren");
            ctx.Transaktionen.Any(t => t.TransaktionId == transaktion.TransaktionId).Should().BeTrue();

            // Storno ist erlaubt: Gegenbuchung entsteht, Transaktion bleibt erhalten.
            var storno = await service.Storno(user, transaktion.TransaktionId, "Fehlbuchung");
            storno.Should().BeOfType<OkResult>();

            ctx.Transaktionen.Any(t => t.TransaktionId == transaktion.TransaktionId).Should().BeTrue();
            var zahlungNachher = await ctx.Buchungssaetze
                .Include(s => s.StornoNach)
                .FirstAsync(s => s.BuchungssatzId == zahlung.BuchungssatzId);
            zahlungNachher.StornoNach.Should().NotBeNull();
        }

        [Fact]
        public async Task StornoOhneGrund_Abgelehnt()
        {
            var (ctx, auth, user) = Setup();
            var transaktion = NeueTransaktion();
            ctx.Transaktionen.Add(transaktion);
            await ctx.SaveChangesAsync();

            var service = MakeService(ctx, auth);
            var result = await service.Storno(user, transaktion.TransaktionId, "  ");

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
