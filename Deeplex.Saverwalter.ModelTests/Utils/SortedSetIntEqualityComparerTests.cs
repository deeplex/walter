using FakeItEasy;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class SortedSetIntEqualityComparerTests
    {
        [Fact()]
        public void EqualsTest()
        {
            var fake1 = A.Fake<SortedSet<int>>();
            var fake2 = new List<int>().ToArray();
            fake1.CopyTo(fake2);

            Equals(fake1, fake2).Should().BeTrue();
        }

        [Fact(Skip = "How? Has no reference anyway...")]
        public void GetHashCodeTest()
        {
            var fake = A.Fake<SortedSet<int>>();
        }
    }
}