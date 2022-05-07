using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class JuristischePersonTests
    {
        [Fact]
        public void JuristischePersonTest()
        {
            var mock = new JuristischePerson();
            mock.Should().NotBeNull();
        }
    }
}