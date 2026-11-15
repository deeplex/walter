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
using static Deeplex.Saverwalter.WebAPI.Services.Buchungen.TransaktionBuchungsService;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ErhaltungsaufwandServiceTests
    {
        private static async Task<(Wohnung wohnung, Kontakt aussteller, Bankkonto bank)> SeedAsync(SaverwalterContext ctx)
        {
            var wohnung = new Wohnung("EA-Wohnung")
            {
                MietErtragskonto = new Buchungskonto("4000", "Mieterträge", BuchungskontoTyp.Ertrag),
                AufwandsKonto = new Buchungskonto("4900", "Aufwand", BuchungskontoTyp.Aufwand),
            };
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh);
            var bank = new Bankkonto
            {
                Iban = "DE100",
                BuchungsKonto = new Buchungskonto("1000", "Bank", BuchungskontoTyp.Aktiv),
            };
            ctx.Wohnungen.Add(wohnung);
            ctx.Kontakte.Add(aussteller);
            ctx.Bankkontos.Add(bank);
            await ctx.SaveChangesAsync();
            return (wohnung, aussteller, bank);
        }

        private static Task<Buchungssatz> RechnungAsync(
            SaverwalterContext ctx, Wohnung wohnung, Kontakt aussteller, decimal betrag, DateOnly datum) =>
            new ErhaltungsaufwendungBuchungsService(ctx).BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, betrag, datum, "Heizungsreparatur", null);

        private static Task<Transaktion> ZahlungAsync(
            SaverwalterContext ctx, Bankkonto bank, ErhaltungsaufwendungsInput ea, DateOnly datum) =>
            new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionsInput
            {
                Betrag = ea.Betrag,
                Zahlungsdatum = datum,
                ZahlerId = bank.BankkontoId,
                Erhaltungsaufwendungen = { ea },
            });

        private static Task<ErhaltungsaufwandService.Erhaltungsaufwand> ErmittleAsync(SaverwalterContext ctx, Wohnung wohnung) =>
            new ErhaltungsaufwandService(ctx).ErmittleAsync(wohnung.WohnungId);

        [Fact]
        public async Task ErfassteRechnungIstTeilweiseBezahlt()
        {
            var ctx = TestUtils.GetContext();
            var (wohnung, aussteller, bank) = await SeedAsync(ctx);
            var satz = await RechnungAsync(ctx, wohnung, aussteller, 500m, new DateOnly(2024, 3, 1));
            var transaktion = await ZahlungAsync(ctx, bank,
                new ErhaltungsaufwendungsInput { ExistingBuchungssatzId = satz.BuchungssatzId, Betrag = 200m },
                new DateOnly(2024, 4, 2));
            ctx.ChangeTracker.Clear();

            var rechnung = (await ErmittleAsync(ctx, wohnung)).Rechnungen.Should().ContainSingle().Subject;

            rechnung.BuchungssatzId.Should().Be(satz.BuchungssatzId);
            rechnung.Aussteller.Should().Be("Handwerker GmbH");
            rechnung.Betrag.Should().Be(500m);
            rechnung.Bezahlt.Should().Be(200m);
            rechnung.Offen.Should().Be(300m);
            rechnung.DirektBezahlt.Should().BeFalse();
            rechnung.KannBezahlen.Should().BeTrue();
            var zahlung = rechnung.Zahlungen.Should().ContainSingle().Subject;
            zahlung.TransaktionId.Should().Be(transaktion.TransaktionId);
            zahlung.Datum.Should().Be(new DateOnly(2024, 4, 2));
            zahlung.Betrag.Should().Be(200m);
            zahlung.Zahler.Should().Be("DE100");
        }

        [Fact]
        public async Task DirektBezahlteRechnungIstBezahlt()
        {
            var ctx = TestUtils.GetContext();
            var (wohnung, _, bank) = await SeedAsync(ctx);
            var transaktion = await ZahlungAsync(ctx, bank,
                new ErhaltungsaufwendungsInput { WohnungId = wohnung.WohnungId, Betrag = 80m, Beschreibung = "Silikon" },
                new DateOnly(2024, 5, 6));
            ctx.ChangeTracker.Clear();

            var rechnung = (await ErmittleAsync(ctx, wohnung)).Rechnungen.Should().ContainSingle().Subject;

            rechnung.DirektBezahlt.Should().BeTrue();
            rechnung.Betrag.Should().Be(80m);
            rechnung.Bezahlt.Should().Be(80m);
            rechnung.Offen.Should().Be(0m);
            rechnung.KannBezahlen.Should().BeFalse();
            rechnung.Zahlungen.Should().ContainSingle()
                .Which.TransaktionId.Should().Be(transaktion.TransaktionId);
        }

        [Fact]
        public async Task StornierteRechnungBleibtAlsBelegOhneOffenenBetrag()
        {
            var ctx = TestUtils.GetContext();
            var (wohnung, aussteller, _) = await SeedAsync(ctx);
            var satz = await RechnungAsync(ctx, wohnung, aussteller, 500m, new DateOnly(2024, 3, 1));
            await new StornoBuchungsService(ctx).StornierenAsync(satz.BuchungssatzId, "doppelt erfasst");
            ctx.ChangeTracker.Clear();

            var ergebnis = await ErmittleAsync(ctx, wohnung);

            var rechnung = ergebnis.Rechnungen.Should().ContainSingle().Subject;
            rechnung.BuchungssatzId.Should().Be(satz.BuchungssatzId);
            rechnung.Storniert.Should().BeTrue();
            rechnung.Offen.Should().Be(0m);
            rechnung.KannBezahlen.Should().BeFalse();
            ergebnis.Jahre.Should().ContainSingle().Which.Erhaltungsaufwand.Should().Be(0m);
        }

        [Fact]
        public async Task JahresueberblickTrenntErhaltungsaufwandVonBetriebskosten()
        {
            var ctx = TestUtils.GetContext();
            var (wohnung, aussteller, _) = await SeedAsync(ctx);
            await RechnungAsync(ctx, wohnung, aussteller, 500m, new DateOnly(2024, 3, 1));
            await RechnungAsync(ctx, wohnung, aussteller, 70m, new DateOnly(2023, 9, 1));

            // Eigenanteil an einer Betriebskostenrechnung (Leerstand): keine Rechnung für Erhaltungsaufwand.
            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Wasser"),
                NkVerrechnungsKonto = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1200", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(umlage);
            var bk = new Buchungssatz(new DateOnly(2024, 6, 1), "Betriebskosten Wasser 2024") { Buchungsjahr = 2024 };
            bk.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 300m) { Buchungssatz = bk, Buchungskonto = umlage.NkVerrechnungsKonto });
            bk.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 120m) { Buchungssatz = bk, Buchungskonto = wohnung.AufwandsKonto });
            ctx.Buchungssaetze.Add(bk);

            var miete = new Buchungssatz(new DateOnly(2024, 1, 1), "Sollstellung") { Buchungsjahr = 2024 };
            miete.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 800m) { Buchungssatz = miete, Buchungskonto = new Buchungskonto("1400", "Miete", BuchungskontoTyp.Aktiv) });
            miete.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 800m) { Buchungssatz = miete, Buchungskonto = wohnung.MietErtragskonto });
            ctx.Buchungssaetze.Add(miete);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var ergebnis = await ErmittleAsync(ctx, wohnung);

            ergebnis.Rechnungen.Select(r => r.Betrag).Should().Equal(500m, 70m);
            ergebnis.Jahre.Select(j => (j.Wert, j.Mieteinnahmen, j.Erhaltungsaufwand, j.BetriebskostenEigenanteil))
                .Should().Equal((2024, 800m, 500m, 120m), (2023, 0m, 70m, 0m));
        }
    }
}
