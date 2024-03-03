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
    public class SortedSetIntEqualityComparerTests
    {
        [Fact]
        public void EqualsTest()
        {
            var mock1 = new SortedSet<int>() { 1, 14, 15, 17 };
            var mock2 = new SortedSet<int>() { 1, 14, 15, 17 };
            var stub = new SortedSetIntEqualityComparer();

            stub.Equals(mock1, mock2).Should().BeTrue();
        }

        [Fact]
        public void EqualsTest2()
        {
            var mock1 = new SortedSet<int>() { 1, 14, 15, 17 };
            var mock2 = new SortedSet<int>() { 17, 15, 1, 14 };
            var stub = new SortedSetIntEqualityComparer();

            stub.Equals(mock1, mock2).Should().BeTrue();
        }

        [Fact]
        public void EqualsTest3()
        {

            var mock1 = new SortedSet<int>() { 1, 14, 15, 17 };
            var mock2 = new SortedSet<int>() { 1, 14, 15, 16 };
            var stub = new SortedSetIntEqualityComparer();


            stub.Equals(mock1, mock2).Should().BeFalse();
        }

        [Fact]
        public void EqualsTest4()
        {
            var mock1 = new SortedSet<int>() { 1, 14, 15, 17 };
            var mock2 = new SortedSet<int>() { 17, 15, 1, 14, 17, 17 };
            var stub = new SortedSetIntEqualityComparer();

            stub.Equals(mock1, mock2).Should().BeTrue();
        }
    }
}
