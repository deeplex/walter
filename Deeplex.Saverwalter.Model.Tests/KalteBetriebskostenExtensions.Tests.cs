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

using Deeplex.Saverwalter.Model;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.ModelTests
{
    public class KalteBetriebskostenExtensionsTests
    {
        [Fact]
        public void AsMinTest()
        {
            var mock = new DateTime();
            var stub = mock.AsMin();
            stub.Should().BeSameDateAs(DateTime.Now.AsUtcKind());
        }

        [Fact]
        public void AsMinTest2()
        {
            var mock = DateTime.Now;
            var stub = mock.AsMin();
            stub.Should().BeSameDateAs(DateTime.Now.AsUtcKind());
        }

        [Fact()]
        public void AsUtcKindTest()
        {
            var mock = DateTime.Now;
            var stub = mock.AsUtcKind();
            stub.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void ToUnitStringTest()
        {
            Zaehlertyp.Kaltwasser.ToUnitString().Should().Be("m³");
            Zaehlertyp.Warmwasser.ToUnitString().Should().Be("m³");
            Zaehlertyp.Strom.ToUnitString().Should().Be("kWh");
            Zaehlertyp.Gas.ToUnitString().Should().Be("kWh");
        }
    }
}
