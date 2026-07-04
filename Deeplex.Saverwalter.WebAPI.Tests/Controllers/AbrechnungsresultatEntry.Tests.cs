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
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    /// <summary>
    /// Saldo-/Ausgeglichen-Ableitung des Abrechnungsresultat-DTOs für die neue
    /// Buchungsform (Nachzahlung als Soll auf BkAbrechnungsKonto), Erstattungen
    /// und das alte Nachzahlungsformat (Soll auf ZahlungsKonto).
    /// </summary>
    public class AbrechnungsresultatEntryTests
    {
        private static (SaverwalterContext ctx, Abrechnungsresultat resultat) SetupGebucht(decimal saldo)
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);

            // Geleistete NK-Vorauszahlungen (Haben auf NkBuchungskonto) — Basis für
            // die abgeleitete Vorauszahlung (1.000 €).
            var vzSatz = new Buchungssatz(new DateOnly(2024, 6, 1), "NK-VZ 2024") { Buchungsjahr = 2024 };
            vzSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 1000m)
            { Buchungssatz = vzSatz, Buchungskonto = vertrag.NkBuchungskonto });
            vzSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 1000m)
            { Buchungssatz = vzSatz, Buchungskonto = vertrag.ZahlungsKonto });
            ctx.Buchungssaetze.Add(vzSatz);

            var resultat = new Abrechnungsresultat { Vertrag = vertrag, Abgesendet = true };
            ctx.Abrechnungsresultate.Add(resultat);
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, 2024, saldo);
            ctx.SaveChanges();
            return (ctx, resultat);
        }

        private static TransaktionBuchungsService.TransaktionsInput AusgleichsInput(
            Abrechnungsresultat resultat, decimal betrag, int? zahlerId = null) => new()
        {
            Betrag = betrag,
            Zahlungsdatum = new DateOnly(2025, 3, 10),
            Verwendungszweck = "NK-Abrechnung",
            ZahlerId = zahlerId,
            AbrechnungsAusgleiche =
            {
                new TransaktionBuchungsService.AbrechnungsAusgleichInput
                {
                    AbrechnungsresultatId = resultat.AbrechnungsresultatId,
                    Betrag = betrag
                }
            }
        };

        [Fact]
        public void NachzahlungOhneZahlungIstOffen()
        {
            var (_, resultat) = SetupGebucht(saldo: 200m);

            var entry = new AbrechnungsresultatEntry(resultat, new Permissions());

            entry.Saldo.Should().Be(200m);
            entry.Rechnungsbetrag.Should().Be(1200m);
            entry.Vorauszahlung.Should().Be(1000m);
            entry.Ausgeglichen.Should().BeFalse();
            entry.OffenerBetrag.Should().Be(200m);
            entry.AusgleichsZahlungen.Should().BeEmpty();
        }

        [Fact]
        public async Task NachzahlungMitTeilzahlungUndVollzahlung()
        {
            var (ctx, resultat) = SetupGebucht(saldo: 200m);
            var service = new TransaktionBuchungsService(ctx);

            await service.BucheAsync(AusgleichsInput(resultat, 150m));
            var teilweise = new AbrechnungsresultatEntry(resultat, new Permissions());
            teilweise.Ausgeglichen.Should().BeFalse();
            teilweise.OffenerBetrag.Should().Be(50m);
            teilweise.AusgleichsZahlungen.Should().ContainSingle(z => z.Betrag == 150m);

            await service.BucheAsync(AusgleichsInput(resultat, 50m));
            var voll = new AbrechnungsresultatEntry(resultat, new Permissions());
            voll.Ausgeglichen.Should().BeTrue();
            voll.OffenerBetrag.Should().Be(0m);
            voll.AusgleichsZahlungen.Should().HaveCount(2);
        }

        [Fact]
        public async Task ErstattungMitAuszahlungIstAusgeglichen()
        {
            var (ctx, resultat) = SetupGebucht(saldo: -200m);
            var zahler = new Bankkonto
            {
                Iban = "DE100",
                BuchungsKonto = new Buchungskonto("100", "Bank", BuchungskontoTyp.Aktiv)
            };
            ctx.Bankkontos.Add(zahler);
            ctx.SaveChanges();

            var offen = new AbrechnungsresultatEntry(resultat, new Permissions());
            offen.Saldo.Should().Be(-200m);
            offen.Ausgeglichen.Should().BeFalse();
            offen.OffenerBetrag.Should().Be(200m);

            await new TransaktionBuchungsService(ctx)
                .BucheAsync(AusgleichsInput(resultat, 200m, zahler.BankkontoId));

            var entry = new AbrechnungsresultatEntry(resultat, new Permissions());
            entry.Ausgeglichen.Should().BeTrue();
            entry.AusgleichsZahlungen.Should().ContainSingle(z => z.Betrag == 200m);
        }

        [Fact]
        public void GuthabenOhneAuszahlungIstOffen()
        {
            var (_, resultat) = SetupGebucht(saldo: -200m);

            var entry = new AbrechnungsresultatEntry(resultat, new Permissions());

            entry.Saldo.Should().Be(-200m);
            entry.Rechnungsbetrag.Should().Be(800m);
            entry.Vorauszahlung.Should().Be(1000m);
            entry.Ausgeglichen.Should().BeFalse();
            entry.OffenerBetrag.Should().Be(200m);
        }

        [Fact]
        public void SaldoNullIstAusgeglichen()
        {
            var (_, resultat) = SetupGebucht(saldo: 0m);

            var entry = new AbrechnungsresultatEntry(resultat, new Permissions());

            entry.Saldo.Should().Be(0m);
            entry.Ausgeglichen.Should().BeTrue();
            entry.OffenerBetrag.Should().Be(0m);
        }
    }
}
