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
using BetriebskostenabrechnungService = Deeplex.Saverwalter.BetriebskostenabrechnungService;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BetriebskostenabrechnungUtilsTests
    {
        private static readonly DateOnly Jan1 = new(2021, 1, 1);
        private static readonly DateOnly Dec31 = new(2021, 12, 31);

        [Fact]
        public void GetMietminderung_NoMinderungen_ReturnsZero()
        {
            var vertrag = new Vertrag();

            var result = BetriebskostenabrechnungService.Utils.GetMietminderung(vertrag, Jan1, Dec31);

            result.Should().Be(0m);
        }

        [Fact]
        public void GetMietminderung_FullYearMinderung_ReturnsMinderungRate()
        {
            var vertrag = new Vertrag();
            vertrag.Mietminderungen.Add(new Mietminderung(Jan1, 0.10m));

            var result = BetriebskostenabrechnungService.Utils.GetMietminderung(vertrag, Jan1, Dec31);

            result.Should().Be(0.10m);
        }

        [Fact]
        public void GetMietminderung_HalfYearMinderung_ReturnsHalfRate()
        {
            // Minderung covers Jul 1 – Dec 31 (184 days out of 365)
            var vertrag = new Vertrag();
            var jul1 = new DateOnly(2021, 7, 1);
            vertrag.Mietminderungen.Add(new Mietminderung(jul1, 0.10m));

            var result = BetriebskostenabrechnungService.Utils.GetMietminderung(vertrag, Jan1, Dec31);

            var expectedDays = Dec31.DayNumber - jul1.DayNumber + 1;
            var totalDays = Dec31.DayNumber - Jan1.DayNumber + 1;
            var expected = 0.10m * expectedDays / totalDays;
            result.Should().BeApproximately(expected, 0.0001m);
        }

        [Fact]
        public void GetMietminderung_MinderungEndedBeforePeriod_ReturnsZero()
        {
            var vertrag = new Vertrag();
            var minderung = new Mietminderung(new DateOnly(2020, 1, 1), 0.20m)
            {
                Ende = new DateOnly(2020, 12, 31)
            };
            vertrag.Mietminderungen.Add(minderung);

            var result = BetriebskostenabrechnungService.Utils.GetMietminderung(vertrag, Jan1, Dec31);

            result.Should().Be(0m);
        }

        [Fact]
        public void GetMietminderung_TwoOverlappingMinderungen_SumsCorrectly()
        {
            var vertrag = new Vertrag();
            // Both cover the full year
            vertrag.Mietminderungen.Add(new Mietminderung(Jan1, 0.05m));
            vertrag.Mietminderungen.Add(new Mietminderung(Jan1, 0.10m));

            var result = BetriebskostenabrechnungService.Utils.GetMietminderung(vertrag, Jan1, Dec31);

            result.Should().BeApproximately(0.15m, 0.0001m);
        }
    }
}
