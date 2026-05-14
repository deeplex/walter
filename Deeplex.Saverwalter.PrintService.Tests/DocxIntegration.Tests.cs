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
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.PrintService.Tests
{
    public class DocxIntegrationTests
    {
        [Fact]
        public void EverythingZeroTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var einheiten = NkGruppenAbrechnungsService.ComputeEinheiten(
                vertrag.Wohnung.Umlagen, 2021);
            var partei = einheiten
                .SelectMany(e => e.Parteien)
                .First(p => p.Vertrag?.VertragId == vertrag.VertragId);
            var druckdaten = NkDruckdaten.Build(partei, einheiten, 2021);

            var stream = new MemoryStream();

            // Act
            druckdaten.SaveAsDocx(stream);

            // Assert
            stream.Length.Should().BeGreaterThan(1000);
        }

        [Fact]
        public void EntwurfHinweisTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var einheiten = NkGruppenAbrechnungsService.ComputeEinheiten(
                vertrag.Wohnung.Umlagen, 2021);
            var partei = einheiten
                .SelectMany(e => e.Parteien)
                .First(p => p.Vertrag?.VertragId == vertrag.VertragId);
            var druckdaten = NkDruckdaten.Build(partei, einheiten, 2021);

            var stream = new MemoryStream();

            // Act
            druckdaten.SaveAsDocx(stream, istEntwurf: true, entwurfGrund: "Noch nicht gebucht");

            // Assert
            stream.Length.Should().BeGreaterThan(1000);
        }
    }
}
