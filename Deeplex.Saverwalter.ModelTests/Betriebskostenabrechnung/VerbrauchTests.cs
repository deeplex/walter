using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class VerbrauchTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0.1)]
        [InlineData(2)]
        [InlineData(100)]
        [InlineData(102)]
        public void VerbrauchTest(double delta)
        {
            var btyp = Betriebskostentyp.Breitbandkabelanschluss;
            var ztyp = Zaehlertyp.Kaltwasser;

            var kennnummer = "mockKennnummer";
            var stub = new Verbrauch(btyp, kennnummer, ztyp, delta);

            stub.Should().NotBeNull();
        }
    }
}