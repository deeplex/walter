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
    public class UmlageRechnungenServiceTests
    {
        private static async Task<(Umlage umlage, Bankkonto bank)> SeedAsync(SaverwalterContext ctx)
        {
            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Wasser"),
                NkVerrechnungsKonto = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1200", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            var bank = new Bankkonto
            {
                Iban = "DE100",
                BuchungsKonto = new Buchungskonto("1000", "Bank", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(umlage);
            ctx.Bankkontos.Add(bank);
            await ctx.SaveChangesAsync();
            return (umlage, bank);
        }

        private static Task<Buchungssatz> RechnungAsync(SaverwalterContext ctx, Umlage umlage, decimal betrag, int jahr) =>
            new BetriebskostenrechnungBuchungsService(ctx).BucheRechnungAsync(
                umlage, betrag, new DateOnly(jahr, 11, 1), jahr, null);

        private static Task<Transaktion> ZahlungAsync(
            SaverwalterContext ctx, Bankkonto bank, Buchungssatz rechnung, decimal betrag) =>
            new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionsInput
            {
                Betrag = betrag,
                Zahlungsdatum = new DateOnly(rechnung.Buchungsjahr, 12, 1),
                ZahlerId = bank.BankkontoId,
                BetriebskostenEingaenge =
                {
                    new BetriebskostenEingangInput { ExistingBuchungssatzId = rechnung.BuchungssatzId, Betrag = betrag }
                },
            });

        private static Task<UmlageRechnungenService.UmlageRechnungen> ErmittleAsync(SaverwalterContext ctx, Umlage umlage) =>
            new UmlageRechnungenService(ctx).ErmittleAsync(umlage.UmlageId);

        [Fact]
        public async Task TeilbezahlteRechnungIstTeilweiseOffen()
        {
            var ctx = TestUtils.GetContext();
            var (umlage, bank) = await SeedAsync(ctx);
            await RechnungAsync(ctx, umlage, 200m, 2023);
            var rechnung = await RechnungAsync(ctx, umlage, 1000m, 2024);
            var transaktion = await ZahlungAsync(ctx, bank, rechnung, 400m);
            ctx.ChangeTracker.Clear();

            var ergebnis = await ErmittleAsync(ctx, umlage);

            ergebnis.Rechnungen.Select(r => r.Jahr).Should().Equal(2024, 2023);
            var neu = ergebnis.Rechnungen[0];
            neu.Betrag.Should().Be(1000m);
            neu.Bezahlt.Should().Be(400m);
            neu.Offen.Should().Be(600m);
            neu.Verteilt.Should().Be(0m);
            neu.KannBezahlen.Should().BeTrue();
            var zahlung = neu.Zahlungen.Should().ContainSingle().Subject;
            zahlung.TransaktionId.Should().Be(transaktion.TransaktionId);
            zahlung.Zahler.Should().Be("DE100");
        }

        [Fact]
        public async Task StornierteZahlungIstRueckgaengigGemachtUndKeineRechnung()
        {
            var ctx = TestUtils.GetContext();
            var (umlage, bank) = await SeedAsync(ctx);
            var rechnung = await RechnungAsync(ctx, umlage, 1000m, 2024);
            var transaktion = await ZahlungAsync(ctx, bank, rechnung, 1000m);
            var zahlungsSatz = ctx.Buchungssaetze.Single(s => s.Transaktion == transaktion);
            await new StornoBuchungsService(ctx).StornierenAsync(zahlungsSatz.BuchungssatzId, "falsch zugeordnet");
            ctx.ChangeTracker.Clear();

            var ergebnis = await ErmittleAsync(ctx, umlage);

            // Der Storno der Zahlung steht im Haben des Verrechnungskontos, ist aber keine Rechnung.
            var einzige = ergebnis.Rechnungen.Should().ContainSingle().Subject;
            einzige.BuchungssatzId.Should().Be(rechnung.BuchungssatzId);
            einzige.Bezahlt.Should().Be(0m);
            einzige.Offen.Should().Be(1000m);
            ergebnis.RueckgaengigGemacht.Should().ContainSingle()
                .Which.TransaktionId.Should().Be(transaktion.TransaktionId);
        }

        [Fact]
        public async Task StornierteVerteilungZaehltNichtAlsVerteilt()
        {
            var ctx = TestUtils.GetContext();
            var (umlage, _) = await SeedAsync(ctx);
            var rechnung = await RechnungAsync(ctx, umlage, 1000m, 2024);
            var nk = new Buchungskonto("1500", "NK Mieter", BuchungskontoTyp.Aktiv);
            var aktiv = new Buchungszeile(SollHaben.Soll, 600m) { Buchungssatz = rechnung, Buchungskonto = nk };
            var storniert = new Buchungszeile(SollHaben.Soll, 400m) { Buchungssatz = rechnung, Buchungskonto = nk };
            rechnung.Buchungszeilen.AddRange([aktiv, storniert]);
            var gegen = new Buchungssatz(new DateOnly(2025, 2, 1), "Storno Abrechnung") { Buchungsjahr = 2024 };
            var gegenZeile = new Buchungszeile(SollHaben.Haben, 400m) { Buchungssatz = gegen, Buchungskonto = nk };
            gegen.Buchungszeilen.Add(gegenZeile);
            ctx.Buchungssaetze.Add(gegen);
            ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich { SollZeile = storniert, HabenZeile = gegenZeile });
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var einzige = (await ErmittleAsync(ctx, umlage)).Rechnungen.Should().ContainSingle().Subject;

            einzige.Verteilt.Should().Be(600m);
        }
    }
}
