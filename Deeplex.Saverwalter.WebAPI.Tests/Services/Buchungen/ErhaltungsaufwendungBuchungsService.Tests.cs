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
    public class ErhaltungsaufwendungBuchungsServiceTests
    {
        private static Wohnung MakeWohnung() => new("EA-Wohnung")
        {
            MietErtragskonto = new Buchungskonto("4000", "Mieterträge", BuchungskontoTyp.Ertrag),
            AufwandsKonto = new Buchungskonto("4900", "Aufwand", BuchungskontoTyp.Aufwand),
        };

        [Fact]
        public async Task BuchtSollAufwandHabenVerbindlichkeit()
        {
            var ctx = TestUtils.GetContext();
            var wohnung = MakeWohnung();
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh);
            ctx.Wohnungen.Add(wohnung);
            ctx.Kontakte.Add(aussteller);
            await ctx.SaveChangesAsync();

            var service = new ErhaltungsaufwendungBuchungsService(ctx);
            var satz = await service.BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, 500m, new DateOnly(2024, 3, 1), "Heizungsreparatur", null);

            satz.Buchungszeilen.Should().HaveCount(2);
            var soll = satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll);
            var haben = satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben);
            soll.Buchungskonto.Should().Be(wohnung.AufwandsKonto);
            soll.Betrag.Should().Be(500m);
            haben.Betrag.Should().Be(500m);
            satz.Beschreibung.Should().Be("Erhaltungsaufwendung: Heizungsreparatur");
        }

        [Fact]
        public async Task LegtVerbindlichkeitsKontoAnWennFehlend()
        {
            var ctx = TestUtils.GetContext();
            var wohnung = MakeWohnung();
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh);
            aussteller.VerbindlichkeitsKonto.Should().BeNull();
            ctx.Wohnungen.Add(wohnung);
            ctx.Kontakte.Add(aussteller);
            await ctx.SaveChangesAsync();

            var service = new ErhaltungsaufwendungBuchungsService(ctx);
            await service.BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, 100m, new DateOnly(2024, 1, 1), "X", null);

            var reloaded = await ctx.Kontakte
                .Include(k => k.VerbindlichkeitsKonto)
                .FirstAsync(k => k.KontaktId == aussteller.KontaktId);
            reloaded.VerbindlichkeitsKonto.Should().NotBeNull();
            reloaded.VerbindlichkeitsKonto!.Kontotyp.Should().Be(BuchungskontoTyp.Passiv);
        }

        [Fact]
        public async Task WiederverwendetVorhandenesVerbindlichkeitsKonto()
        {
            var ctx = TestUtils.GetContext();
            var wohnung = MakeWohnung();
            var konto = new Buchungskonto("VK-1", "Verbindlichkeit", BuchungskontoTyp.Passiv);
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh) { VerbindlichkeitsKonto = konto };
            ctx.Wohnungen.Add(wohnung);
            ctx.Kontakte.Add(aussteller);
            await ctx.SaveChangesAsync();

            var service = new ErhaltungsaufwendungBuchungsService(ctx);
            var satz = await service.BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, 100m, new DateOnly(2024, 1, 1), "X", null);

            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben)
                .Buchungskonto.Should().Be(konto);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task WirftBeiBetragKleinerGleichNull(decimal betrag)
        {
            var ctx = TestUtils.GetContext();
            var wohnung = MakeWohnung();
            var aussteller = new Kontakt("H", Rechtsform.gmbh);
            ctx.Wohnungen.Add(wohnung);
            ctx.Kontakte.Add(aussteller);
            await ctx.SaveChangesAsync();

            var service = new ErhaltungsaufwendungBuchungsService(ctx);
            var act = () => service.BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, betrag, new DateOnly(2024, 1, 1), "X", null);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
