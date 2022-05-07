using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class AnhangExtensionsTests
    {
        [Fact]
        public void getPathTest()
        {
            var mock = new Anhang();
            var path = mock.getPath("some string");
            path.Should().NotBeNull();
        }
    }
}