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

        [Fact(Skip = "How? Has no reference anyway...")]
        public void GetHashCodeTest()
        {
            // TODO
        }
    }
}