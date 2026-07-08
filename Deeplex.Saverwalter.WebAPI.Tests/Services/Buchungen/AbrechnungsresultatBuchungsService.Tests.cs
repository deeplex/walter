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
        public void NachzahlungGlattstelltAufBkAbrechnungskontoGegenNkKonto()
        {
            var (ctx, resultat) = Setup();
            var service = new AbrechnungsresultatBuchungsService(ctx);

            // saldo > 0 → Nachzahlung: Soll BkAbrechnungsKonto / Haben NkBuchungskonto.
            service.BucheAbrechnung(resultat, 2024, saldo: 200m);

            resultat.Buchungssatz.Should().NotBeNull();
            var satz = resultat.Buchungssatz;
            satz.Buchungsdatum.Should().Be(new DateOnly(2024, 12, 31));
            satz.Buchungszeilen.Should().HaveCount(2);
            satz.Buchungszeilen.Single(z =>
                z.SollHaben == SollHaben.Soll
                && z.Buchungskonto == resultat.Vertrag.BkAbrechnungsKonto)
                .Betrag.Should().Be(200m);
            satz.Buchungszeilen.Single(z =>
                z.SollHaben == SollHaben.Haben
                && z.Buchungskonto == resultat.Vertrag.NkBuchungskonto)
                .Betrag.Should().Be(200m);
        }

        [Fact]
        public void GuthabenGlattstelltAufBkAbrechnungskontoGegenNkKonto()
        {
            var (ctx, resultat) = Setup();
            var service = new AbrechnungsresultatBuchungsService(ctx);

            // saldo < 0 → Guthaben: Soll NkBuchungskonto / Haben BkAbrechnungsKonto.
            service.BucheAbrechnung(resultat, 2024, saldo: -200m);

            var satz = resultat.Buchungssatz;
            satz.Buchungszeilen.Should().HaveCount(2);
            satz.Buchungszeilen.Single(z =>
                z.SollHaben == SollHaben.Haben
                && z.Buchungskonto == resultat.Vertrag.BkAbrechnungsKonto)
                .Betrag.Should().Be(200m);
            satz.Buchungszeilen.Single(z =>
                z.SollHaben == SollHaben.Soll
                && z.Buchungskonto == resultat.Vertrag.NkBuchungskonto)
                .Betrag.Should().Be(200m);
            satz.Buchungszeilen.Should().NotContain(z =>
                z.Buchungskonto == resultat.Vertrag.ZahlungsKonto);
        }

        [Fact]
        public void NullSaldoErzeugtLeerenBeleg()
        {
            var (ctx, resultat) = Setup();
            var service = new AbrechnungsresultatBuchungsService(ctx);

            service.BucheAbrechnung(resultat, 2024, saldo: 0m);

            resultat.Buchungssatz.Buchungszeilen.Should().BeEmpty();
        }

        [Fact]
        public void BuchtNichtDoppeltWennBereitsGebucht()
        {
            var (ctx, resultat) = Setup();
            var service = new AbrechnungsresultatBuchungsService(ctx);

            service.BucheAbrechnung(resultat, 2024, 200m);
            var ersterSatz = resultat.Buchungssatz;
            service.BucheAbrechnung(resultat, 2024, 9999m);

            resultat.Buchungssatz.Should().BeSameAs(ersterSatz);
        }
    }
}
