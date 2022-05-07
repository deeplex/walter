using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class VertragTests
    {
        [Fact]
        public void VertragTest()
        {
            var mock = new Vertrag();
            mock.Should().NotBeNull();
        }
    }
}