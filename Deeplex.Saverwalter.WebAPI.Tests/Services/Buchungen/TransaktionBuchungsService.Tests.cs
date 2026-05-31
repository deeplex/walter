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
    public class TransaktionBuchungsServiceTests
    {
        private static Bankkonto MakeBankkonto(string nr) => new()
        {
            Iban = $"DE{nr}",
            BuchungsKonto = new Buchungskonto(nr, "Bank", BuchungskontoTyp.Aktiv),
        };

        // ── Mietzahlung ────────────────────────────────────────────────────────

        [Fact]
        public async Task MietzahlungLegtSollstellungAnUndGleichtAus()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 500m,
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Verwendungszweck = "",
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                        NkVorauszahlung = 0m,
                    }
                }
            };

            var transaktion = await service.BucheAsync(input);

            transaktion.Betrag.Should().Be(500m);
            // Sollstellung (Mietsoll) + Zahlung gebucht
            var saetze = await ctx.Buchungssaetze.Include(s => s.Buchungszeilen).ToListAsync();
            saetze.Should().Contain(s => s.Beschreibung.Contains("Mietsoll"));
            saetze.Should().Contain(s => s.Beschreibung.Contains("Kaltmiete"));
            // OPOS-Ausgleich zwischen Sollstellung und Zahlung
            (await ctx.OffenePostenAusgleiche.CountAsync()).Should().Be(1);
            // Verwendungszweck automatisch erzeugt
            transaktion.Verwendungszweck.Should().Contain("Miete 01/2024");
        }

        [Fact]
        public async Task MietzahlungMitNkVorauszahlungBuchtZweiSaetze()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 650m,
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Verwendungszweck = "Miete",
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                        NkVorauszahlung = 150m,
                    }
                }
            };

            await service.BucheAsync(input);

            var saetze = await ctx.Buchungssaetze.ToListAsync();
            saetze.Should().Contain(s => s.Beschreibung.Contains("NK-VZ"));
        }

        [Fact]
        public async Task WirftWennSummeNichtZumBetragPasst()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 999m, // passt nicht zu Kaltmiete 500
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                    }
                }
            };

            var act = () => service.BucheAsync(input);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task WirftWennKeinePositionMitBetrag()
        {
            var ctx = TestUtils.GetContext();
            var service = new TransaktionBuchungsService(ctx);
            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 0m,
                Zahlungsdatum = new DateOnly(2024, 1, 1),
            };

            var act = () => service.BucheAsync(input);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task WirftWennZahlungVollstaendigeForderungUebersteigt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);

            // Erste Zahlung gleicht die Sollstellung vollständig aus.
            await service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 500m,
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                    }
                }
            });

            // Zweite Zahlung für denselben Monat → übersteigt verbleibende Forderung.
            var act = () => service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 500m,
                Zahlungsdatum = new DateOnly(2024, 1, 6),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                    }
                }
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // ── Sonstiger Buchungssatz ─────────────────────────────────────────────

        [Fact]
        public async Task SonstigerBuchungssatzBrauchtZahlerUndEmpfaenger()
        {
            var ctx = TestUtils.GetContext();
            var service = new TransaktionBuchungsService(ctx);

            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 100m,
                Zahlungsdatum = new DateOnly(2024, 1, 1),
                Sonstige = { new TransaktionBuchungsService.SonstigerBuchungssatzInput { Betrag = 100m, Beschreibung = "X" } }
            };

            var act = () => service.BucheAsync(input);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SonstigerBuchungssatzBuchtZwischenBankkonten()
        {
            var ctx = TestUtils.GetContext();
            var zahler = MakeBankkonto("100");
            var empfaenger = MakeBankkonto("200");
            ctx.Bankkontos.AddRange(zahler, empfaenger);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var transaktion = await service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 100m,
                Zahlungsdatum = new DateOnly(2024, 1, 1),
                ZahlerId = zahler.BankkontoId,
                ZahlungsempfaengerId = empfaenger.BankkontoId,
                Verwendungszweck = "Übertrag",
                Sonstige = { new TransaktionBuchungsService.SonstigerBuchungssatzInput { Betrag = 100m, Beschreibung = "Übertrag" } }
            });

            var satz = await ctx.Buchungssaetze.Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .SingleAsync();
            satz.Buchungszeilen.Should().HaveCount(2);
            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll).Buchungskonto
                .Should().Be(empfaenger.BuchungsKonto);
            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben).Buchungskonto
                .Should().Be(zahler.BuchungsKonto);
        }
    }
}
