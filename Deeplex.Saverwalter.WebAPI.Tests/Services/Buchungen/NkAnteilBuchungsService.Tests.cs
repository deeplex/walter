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
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.NkGruppenAbrechnungsService;

namespace Deeplex.Saverwalter.WebAPI.Tests.Buchungen
{
    public class NkAnteilBuchungsServiceTests
    {
        // ── BucheVertragsNkAnteilAsync ─────────────────────────────────────────

        [Fact]
        public async Task BuchtSollNkKontoHabenVerrechnungsKonto()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Wasser"),
                NkVerrechnungsKonto = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1200", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(umlage);
            await ctx.SaveChangesAsync();

            var service = new NkAnteilBuchungsService(ctx);
            var satz = await service.BucheVertragsNkAnteilAsync(
                vertrag.VertragId, umlage.UmlageId, 120m, 2024, new DateOnly(2024, 12, 31), null);

            satz.Buchungszeilen.Should().HaveCount(2);
            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll)
                .Buchungskonto.Should().Be(vertrag.NkBuchungskonto);
            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben)
                .Buchungskonto.Should().Be(umlage.NkVerrechnungsKonto);
            satz.Beschreibung.Should().StartWith(NkAnteilBuchungsService.BeschreibungPrefix);
        }

        [Fact]
        public async Task VertragsNkAnteilWirftBeiBetragKleinerGleichNull()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new NkAnteilBuchungsService(ctx);
            var act = () => service.BucheVertragsNkAnteilAsync(vertrag.VertragId, 1, 0m, 2024, new DateOnly(2024, 1, 1), null);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task VertragsNkAnteilWirftBeiUnbekanntemVertrag()
        {
            var ctx = TestUtils.GetContext();
            var service = new NkAnteilBuchungsService(ctx);
            var act = () => service.BucheVertragsNkAnteilAsync(999, 1, 100m, 2024, new DateOnly(2024, 1, 1), null);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        // ── BucheAnteileAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task BuchtAnteileAlsSollZeilen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var nkVk = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv);
            var satz = new Buchungssatz(new DateOnly(2024, 12, 31), "Betriebskosten Wasser 2024") { Buchungsjahr = 2024 };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 300m) { Buchungssatz = satz, Buchungskonto = nkVk });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var partei = new NkPartei { Wohnung = vertrag.Wohnung, Vertrag = vertrag };
            var anteile = new List<NkRechnungsAnteil> { new() { Partei = partei, Betrag = 300m } };

            var service = new NkAnteilBuchungsService(ctx);
            var result = await service.BucheAnteileAsync(satz, anteile);

            result.GebuchteAnteile.Should().Be(1);
            satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll)
                .Should().ContainSingle()
                .Which.Buchungskonto.Should().Be(vertrag.NkBuchungskonto);
        }

        [Fact]
        public async Task UeberspringtBereitsGebuchteKontenUndNullBetraege()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var nkVk = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv);
            var satz = new Buchungssatz(new DateOnly(2024, 12, 31), "BK") { Buchungsjahr = 2024 };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 300m) { Buchungssatz = satz, Buchungskonto = nkVk });
            // Bereits eine Soll-Zeile auf das NkBuchungskonto des Vertrags gebucht
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 100m) { Buchungssatz = satz, Buchungskonto = vertrag.NkBuchungskonto });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var partei = new NkPartei { Wohnung = vertrag.Wohnung, Vertrag = vertrag };
            var anteile = new List<NkRechnungsAnteil>
            {
                new() { Partei = partei, Betrag = 200m },  // Konto schon gebucht → übersprungen
            };

            var service = new NkAnteilBuchungsService(ctx);
            var result = await service.BucheAnteileAsync(satz, anteile);

            result.GebuchteAnteile.Should().Be(0);
            result.UebersprungeneAnteile.Should().Be(1);
        }

        [Fact]
        public async Task EigenanteilBuchtAufAufwandsKonto()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var nkVk = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv);
            var satz = new Buchungssatz(new DateOnly(2024, 12, 31), "BK") { Buchungsjahr = 2024 };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 50m) { Buchungssatz = satz, Buchungskonto = nkVk });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            // Eigenanteil-Partei (Vertrag == null) → Buchungskonto == Wohnung.AufwandsKonto
            var eigen = new NkPartei { Wohnung = vertrag.Wohnung, Vertrag = null };
            var anteile = new List<NkRechnungsAnteil> { new() { Partei = eigen, Betrag = 50m } };

            var service = new NkAnteilBuchungsService(ctx);
            await service.BucheAnteileAsync(satz, anteile);

            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll)
                .Buchungskonto.Should().Be(vertrag.Wohnung.AufwandsKonto);
        }
    }
}
