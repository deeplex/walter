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
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests.Buchungen
{
    public class BetriebskostenrechnungBuchungsServiceTests
    {
        private static Umlage MakeUmlage() => new()
        {
            Typ = new Umlagetyp("Wasser"),
            NkVerrechnungsKonto = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv),
            ZahlungsKonto = new Buchungskonto("1200", "Zahlung", BuchungskontoTyp.Aktiv),
        };

        [Fact]
        public async Task BuchtEineHabenZeileAufNkVerrechnungsKonto()
        {
            var ctx = TestUtils.GetContext();
            var umlage = MakeUmlage();
            ctx.Umlagen.Add(umlage);
            await ctx.SaveChangesAsync();

            var service = new BetriebskostenrechnungBuchungsService(ctx);
            var satz = await service.BucheRechnungAsync(umlage, 1200m, new DateOnly(2024, 6, 1), 2024, "Jahresrechnung");

            satz.Buchungszeilen.Should().ContainSingle();
            var zeile = satz.Buchungszeilen.Single();
            zeile.SollHaben.Should().Be(SollHaben.Haben);
            zeile.Betrag.Should().Be(1200m);
            zeile.Buchungskonto.Should().Be(umlage.NkVerrechnungsKonto);
            satz.Buchungsjahr.Should().Be(2024);
            satz.Notiz.Should().Be("Jahresrechnung");
        }

        [Fact]
        public async Task BuchtEineSollZeileAufNkVerrechnungsKontoFuerGutschrift()
        {
            var ctx = TestUtils.GetContext();
            var umlage = MakeUmlage();
            ctx.Umlagen.Add(umlage);
            await ctx.SaveChangesAsync();

            var service = new BetriebskostenrechnungBuchungsService(ctx);
            var satz = await service.BucheRechnungAsync(umlage, -300m, new DateOnly(2024, 6, 1), 2024, "Gutschrift");

            satz.Buchungszeilen.Should().ContainSingle();
            var zeile = satz.Buchungszeilen.Single();
            zeile.SollHaben.Should().Be(SollHaben.Soll);
            zeile.Betrag.Should().Be(300m);
            zeile.Buchungskonto.Should().Be(umlage.NkVerrechnungsKonto);
        }

        [Fact]
        public async Task BuchtEineHabenZeileBeiBetragNull()
        {
            var ctx = TestUtils.GetContext();
            var umlage = MakeUmlage();
            ctx.Umlagen.Add(umlage);
            await ctx.SaveChangesAsync();

            var service = new BetriebskostenrechnungBuchungsService(ctx);
            var satz = await service.BucheRechnungAsync(umlage, 0m, new DateOnly(2024, 1, 1), 2024, null);

            satz.Buchungszeilen.Should().ContainSingle();
            satz.Buchungszeilen.Single().SollHaben.Should().Be(SollHaben.Haben);
            satz.Buchungszeilen.Single().Betrag.Should().Be(0m);
        }

        [Fact]
        public async Task AktualisiereBuchungssatzAendertBetragUndMetadaten()
        {
            var ctx = TestUtils.GetContext();
            var umlage = MakeUmlage();
            ctx.Umlagen.Add(umlage);
            await ctx.SaveChangesAsync();

            var service = new BetriebskostenrechnungBuchungsService(ctx);
            var satz = await service.BucheRechnungAsync(umlage, 1000m, new DateOnly(2024, 1, 1), 2024, null);

            await service.AktualisiereBuchungssatzAsync(
                satz, umlage, 1500m, new DateOnly(2024, 2, 2), 2024, "korrigiert");

            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben).Betrag.Should().Be(1500m);
            satz.Buchungsdatum.Should().Be(new DateOnly(2024, 2, 2));
            satz.Notiz.Should().Be("korrigiert");
        }

        [Fact]
        public async Task AktualisiereAendertSollHabenBeiBetragWechselZuGutschrift()
        {
            var ctx = TestUtils.GetContext();
            var umlage = MakeUmlage();
            ctx.Umlagen.Add(umlage);
            await ctx.SaveChangesAsync();

            var service = new BetriebskostenrechnungBuchungsService(ctx);
            var satz = await service.BucheRechnungAsync(umlage, 1000m, new DateOnly(2024, 1, 1), 2024, null);

            await service.AktualisiereBuchungssatzAsync(satz, umlage, -400m, new DateOnly(2024, 2, 1), 2024, "korrigiert zu Gutschrift");

            var zeile = satz.Buchungszeilen.Single(z => z.Buchungskonto == umlage.NkVerrechnungsKonto);
            zeile.SollHaben.Should().Be(SollHaben.Soll);
            zeile.Betrag.Should().Be(400m);
        }
    }
}
