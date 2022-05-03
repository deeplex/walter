using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class UmlageSchluesselExtensionsTests
    {
        [Fact]
        public void ToDescriptionStringTest()
        {
            for (var i = 0; i < 5; ++i)
            {
                var stub = (UmlageSchluessel)i;
                stub.ToDescriptionString().Should().NotBeNull();
            }
        }
    }
}