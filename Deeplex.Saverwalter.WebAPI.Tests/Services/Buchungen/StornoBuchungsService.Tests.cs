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

using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests.Buchungen
{
    public class StornoBuchungsServiceTests
    {
        private static async Task<(SaverwalterContext ctx, Buchungssatz satz)> SetupGebuchterSatz()
        {
            var ctx = TestUtils.GetContext();
            var soll = new Buchungskonto("1000", "Soll", BuchungskontoTyp.Aktiv);
            var haben = new Buchungskonto("4000", "Haben", BuchungskontoTyp.Ertrag);
            var satz = new Buchungssatz(new DateOnly(2024, 5, 1), "Originalbuchung");
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 300m) { Buchungssatz = satz, Buchungskonto = soll });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 300m) { Buchungssatz = satz, Buchungskonto = haben });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();
            return (ctx, satz);
        }

        [Fact]
        public async Task StorniertMitUmgekehrtenSeiten()
        {
            var (ctx, original) = await SetupGebuchterSatz();
            var service = new StornoBuchungsService(ctx);

            var storno = await service.StornierenAsync(original.BuchungssatzId);

            storno.StornoVon.Should().Be(original);
            storno.Beschreibung.Should().Be("Storno: Originalbuchung");
            storno.Buchungsjahr.Should().Be(original.Buchungsjahr);

            var originalSoll = original.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll);
            var stornoFuerSoll = storno.Buchungszeilen.Single(z => z.Buchungskonto == originalSoll.Buchungskonto);
            stornoFuerSoll.SollHaben.Should().Be(SollHaben.Haben);
            stornoFuerSoll.Betrag.Should().Be(300m);
        }

        [Fact]
        public async Task EntferntOposAusgleicheDerOriginalzeilen()
        {
            var (ctx, original) = await SetupGebuchterSatz();
            // OPOS-Ausgleich auf Originalzeilen anlegen
            var soll = original.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll);
            var haben = original.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben);
            ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich { SollZeile = soll, HabenZeile = haben });
            await ctx.SaveChangesAsync();
            ctx.OffenePostenAusgleiche.Should().HaveCount(1);

            var service = new StornoBuchungsService(ctx);
            await service.StornierenAsync(original.BuchungssatzId);

            (await ctx.OffenePostenAusgleiche.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task WirftBeiUnbekanntemSatz()
        {
            var ctx = TestUtils.GetContext();
            var service = new StornoBuchungsService(ctx);
            var act = () => service.StornierenAsync(Guid.NewGuid());
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task WirftBeiDoppeltemStorno()
        {
            var (ctx, original) = await SetupGebuchterSatz();
            var service = new StornoBuchungsService(ctx);
            await service.StornierenAsync(original.BuchungssatzId);

            var act = () => service.StornierenAsync(original.BuchungssatzId);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
