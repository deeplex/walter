using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class AnhangTests
    {
        [Fact()]
        public void AnhangTest()
        {
            var stub = new Anhang();
            stub.Should().NotBeNull();
            stub.AnhangId.Should().NotBeEmpty();
        }
    }
}