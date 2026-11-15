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
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class KontorollenTests
    {
        [Fact]
        public async Task VerknuepfungenNennenBesitzerUndFunktion()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Wasser"),
                NkVerrechnungsKonto = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv),
                NkSonderVerrechnungsKonto = new Buchungskonto("7001", "NK-Sonder", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1200", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(umlage);
            var bank = new Bankkonto { Iban = "DE100", BuchungsKonto = new Buchungskonto("1000", "Bank", BuchungskontoTyp.Aktiv) };
            ctx.Bankkontos.Add(bank);
            await ctx.SaveChangesAsync();

            var v = await Kontorollen.VerknuepfungenAsync(ctx,
            [
                vertrag.MietBuchungskonto.BuchungskontoId,
                vertrag.ZahlungsKonto.BuchungskontoId,
                umlage.NkVerrechnungsKonto.BuchungskontoId,
                umlage.NkSonderVerrechnungsKonto.BuchungskontoId,
                bank.BuchungsKonto.BuchungskontoId,
            ]);

            KontoVerknuepfungEntry Fuer(Buchungskonto k) => v.Single(x => x.KontoId == k.BuchungskontoId);
            Fuer(vertrag.MietBuchungskonto).Should().BeEquivalentTo(new
            {
                Typ = "Vertrag",
                Id = vertrag.VertragId.ToString(),
                Text = Kontorollen.VertragText(vertrag),
                Funktion = KontoFunktion.Mietforderungen.Name,
                Ausgleichbar = true,
            });
            Fuer(vertrag.ZahlungsKonto).Funktion.Should().Be(KontoFunktion.Zahlungseingaenge.Name);
            Fuer(umlage.NkVerrechnungsKonto).Funktion.Should().Be(KontoFunktion.NkVerrechnung.Name);
            Fuer(umlage.NkSonderVerrechnungsKonto).Funktion.Should().Be(KontoFunktion.NkSonderVerrechnung.Name);
            Fuer(bank.BuchungsKonto).Should().BeEquivalentTo(new
            {
                Typ = "Bankkonto",
                Id = bank.BankkontoId.ToString(),
                Text = "DE100",
                Funktion = KontoFunktion.Bankkonto.Name,
                Ausgleichbar = false,
            });
        }
    }
}
