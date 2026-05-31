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
    public class AbrechnungsresultatBuchungsServiceTests
    {
        private static (SaverwalterContext ctx, Abrechnungsresultat resultat) Setup()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            ctx.SaveChanges();
            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            return (ctx, resultat);
        }

        [Fact]
        public void NachzahlungBuchtSaldoAlsSollAufZahlungskonto()
        {
            var (ctx, resultat) = Setup();
            var service = new AbrechnungsresultatBuchungsService(ctx);

            // saldo > 0 → Nachzahlung (Mieter zahlt) → Soll Zahlungskonto
            service.BucheAbrechnung(resultat, 2024, vorauszahlung: 1000m, rechnungsbetrag: 1200m, saldo: 200m);

            resultat.Buchungssatz.Should().NotBeNull();
            var satz = resultat.Buchungssatz;
            satz.Buchungsdatum.Should().Be(new DateOnly(2024, 12, 31));
            satz.Buchungszeilen.Should().HaveCount(3);
            satz.Buchungszeilen.Single(z => z.Buchungskonto == resultat.Vertrag.ZahlungsKonto)
                .SollHaben.Should().Be(SollHaben.Soll);
            // Soll == Haben (doppelte Buchführung)
            var soll = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag);
            var haben = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag);
            soll.Should().Be(haben);
        }

        [Fact]
        public void ErstattungBuchtSaldoAlsHabenAufZahlungskonto()
        {
            var (ctx, resultat) = Setup();
            var service = new AbrechnungsresultatBuchungsService(ctx);

            // saldo < 0 → Erstattung (Vermieter zahlt) → Haben Zahlungskonto
            service.BucheAbrechnung(resultat, 2024, vorauszahlung: 1200m, rechnungsbetrag: 1000m, saldo: -200m);

            var satz = resultat.Buchungssatz;
            satz.Buchungszeilen.Single(z => z.Buchungskonto == resultat.Vertrag.ZahlungsKonto)
                .SollHaben.Should().Be(SollHaben.Haben);
            var soll = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag);
            var haben = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag);
            soll.Should().Be(haben);
        }

        [Fact]
        public void NullSaldoErzeugtNurZweiZeilen()
        {
            var (ctx, resultat) = Setup();
            var service = new AbrechnungsresultatBuchungsService(ctx);

            service.BucheAbrechnung(resultat, 2024, vorauszahlung: 1000m, rechnungsbetrag: 1000m, saldo: 0m);

            resultat.Buchungssatz.Buchungszeilen.Should().HaveCount(2);
        }

        [Fact]
        public void BuchtNichtDoppeltWennBereitsGebucht()
        {
            var (ctx, resultat) = Setup();
            var service = new AbrechnungsresultatBuchungsService(ctx);

            service.BucheAbrechnung(resultat, 2024, 1000m, 1200m, 200m);
            var ersterSatz = resultat.Buchungssatz;
            service.BucheAbrechnung(resultat, 2024, 9999m, 9999m, 9999m);

            resultat.Buchungssatz.Should().BeSameAs(ersterSatz);
        }
    }
}
