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

using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AbrechnungsschutzServiceTests
    {
        [Theory]
        [InlineData(6, new[] { 2024 })]        // Mitte des Jahres → nur dieses Jahr
        [InlineData(12, new[] { 2024, 2025 })] // Dezember → auch Anfangsstand des Folgejahres
        [InlineData(1, new[] { 2024, 2023 })]  // Januar → auch Endstand-Ersatz des Vorjahres
        public void StandBetroffeneJahre_BeachtetJahresgrenzen(int monat, int[] erwartet)
        {
            var jahre = AbrechnungsschutzService
                .StandBetroffeneJahre(new DateOnly(2024, monat, 15)).ToHashSet();
            jahre.Should().BeEquivalentTo(erwartet);
        }

        [Fact]
        public void BetroffeneAbgerechneteJahre_NimmtNurJahreAbBetroffen()
        {
            var abgerechnet = new HashSet<int> { 2021, 2023, 2024 };
            // Änderung wirkt ab 2023 → 2021 bleibt unberührt.
            AbrechnungsschutzService.BetroffeneAbgerechneteJahre(abgerechnet, 2023)
                .Should().Equal(2023, 2024);
            // Änderung nur ab 2025 → nichts betroffen.
            AbrechnungsschutzService.BetroffeneAbgerechneteJahre(abgerechnet, 2025)
                .Should().BeEmpty();
        }

        [Fact]
        public void Schnittmenge_TrifftNurAbgerechneteStandjahre()
        {
            var abgerechnet = new HashSet<int> { 2023 };
            // Dezember-2023-Stand betrifft 2023 (abgerechnet) und 2024 (nicht) → 2023.
            AbrechnungsschutzService.Schnittmenge(
                abgerechnet, AbrechnungsschutzService.StandBetroffeneJahre(new DateOnly(2023, 12, 31)))
                .Should().Equal(2023);
            // Mitten-2025-Stand betrifft 2025 (nicht abgerechnet) → leer.
            AbrechnungsschutzService.Schnittmenge(
                abgerechnet, AbrechnungsschutzService.StandBetroffeneJahre(new DateOnly(2025, 6, 1)))
                .Should().BeEmpty();
        }
    }
}
