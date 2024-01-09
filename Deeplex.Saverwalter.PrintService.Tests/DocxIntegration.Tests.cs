// Copyright (c) 2023-2024 Kai Lawrence
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

            var abrechnung = new Betriebskostenabrechnung(
                vertrag,
                2021,
                new DateOnly(2021, 1, 1),
                new DateOnly(2021, 12, 31)
            );

            var stream = new MemoryStream();

            // Act
            abrechnung.SaveAsDocx(stream);

            // Assert
            stream.Length.Should().BeGreaterThan(1000);
        }
    }
}
