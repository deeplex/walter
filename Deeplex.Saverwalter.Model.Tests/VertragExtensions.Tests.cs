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

using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class VertragExtensionsTests
    {
        [Fact]
        public void BeginnAndEndTest1()
        {
            var wohnung = A.Fake<Wohnung>();
            var vertrag = new Vertrag()
            {
                Wohnung = wohnung,
            };

            var date1 = new DateOnly(2000, 1, 1);
            var date2 = new DateOnly(2001, 1, 1);
            var version1 = new VertragVersion(date1, 100, 1)
            {
                Vertrag = vertrag
            };
            var version2 = new VertragVersion(date2, 100, 1)
            {
                Vertrag = vertrag
            };

            vertrag.Versionen.Add(version1);
            vertrag.Versionen.Add(version2);


            vertrag.Beginn().Should().Be(date1);
            version1.Ende().Should().Be(date2.AddDays(-1));
            version2.Ende().Should().BeNull();
        }

        [Fact]
        public void BeginnAndEndTest2()
        {
            var wohnung = A.Fake<Wohnung>();

            var date0 = new DateOnly(2002, 5, 31);

            var vertrag = new Vertrag()
            {
                Wohnung = wohnung,
                Ende = date0
            };

            var date1 = new DateOnly(2000, 1, 1);
            var date2 = new DateOnly(2001, 1, 1);
            var version1 = new VertragVersion(date1, 100, 1)
            {
                Vertrag = vertrag
            };
            var version2 = new VertragVersion(date2, 100, 1)
            {
                Vertrag = vertrag
            };

            vertrag.Versionen.Add(version1);
            vertrag.Versionen.Add(version2);

            vertrag.Beginn().Should().Be(date1);
            version1.Ende().Should().Be(date2.AddDays(-1));
            version2.Ende().Should().Be(date0);
        }
    }
}
