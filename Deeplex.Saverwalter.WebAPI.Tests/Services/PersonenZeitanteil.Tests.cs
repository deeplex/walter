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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class PersonenZeitanteilTests
    {
        // bDay/bMonth/bYear = Nutzungsbeginn der Partei
        // eDay/eMonth/eYear = Nutzungsende der Partei
        // vDay/vMonth/vYear = Beginn der VertragVersion (für Zeitraum-Berechnung)
        // personenzahl = Anzahl Personen dieser Partei
        // gesamtPersonenzahl = Summe aller Personen in der Einheit
        // year = Abrechnungsjahr
        [Theory]
        [InlineData(1, 1, 2020, 31, 12, 2020, 1, 1, 2019, 1, 4, 2020, 0.25)]
        [InlineData(1, 1, 2020,  1,  7, 2020, 1, 1, 2019, 1, 4, 2020, 0.125)]
        [InlineData(1, 1, 2020,  1,  7, 2020, 1, 1, 2019, 0, 2, 2020, 0.0)]
        [InlineData(1, 1, 2020,  1,  7, 2020, 1, 1, 2019, 0, 0, 2020, 0.0)]
        public void PersonenZeitanteilTest(
            int bDay, int bMonth, int bYear,
            int eDay, int eMonth, int eYear,
            int vDay, int vMonth, int vYear,
            int personenzahl,
            int gesamtPersonenzahl,
            int year,
            double expectedAnteil)
        {
            var beginn = new DateOnly(bYear, bMonth, bDay);
            var ende = new DateOnly(eYear, eMonth, eDay);

            var vertragBeginn = new DateOnly(vYear, vMonth, vDay);
            var version = new VertragVersion(vertragBeginn, 1000, personenzahl);
            var vertrag = new Vertrag();
            vertrag.Versionen.Add(version);

            var zeitraum = new Zeitraum(year, vertrag);

            var output = new PersonenZeitanteil(beginn, ende, personenzahl, gesamtPersonenzahl, zeitraum);

            output.Anteil.Should().BeApproximately((decimal)expectedAnteil, 0.0001m);
        }
    }
}
