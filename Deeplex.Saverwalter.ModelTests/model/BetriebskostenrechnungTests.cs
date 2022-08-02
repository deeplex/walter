using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class BetriebskostenrechnungTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(200)]
        public void ShallowCopyTest(int betrag)
        {
            var mock = new Betriebskostenrechnung();
            mock.Betrag = betrag;
            var copy = mock.ShallowCopy();
            copy.Betrag.Should().Be(betrag);
        }
    }
}