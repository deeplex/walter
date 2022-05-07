using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class NatuerlichePersonTests
    {
        [Fact]
        public void NatuerlichePersonTest()
        {
            var mock = new NatuerlichePerson();
            mock.Should().NotBeNull();
        }
    }
}