using FakeItEasy;
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

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(2000, 100)]
        public void NewYearTest(int betrag, int jahr)
        {
            var mock = new Betriebskostenrechnung();
            mock.Betrag = betrag;
            mock.BetreffendesJahr = jahr;

            var newYear = mock.NewYear();
            newYear.BetreffendesJahr.Should().Be(jahr + 1);
            newYear.Betrag.Should().Be(0);
            newYear.BetriebskostenrechnungId.Should().Be(0);
            newYear.Wohnungen.Should().BeEmpty();
        }
    }
}