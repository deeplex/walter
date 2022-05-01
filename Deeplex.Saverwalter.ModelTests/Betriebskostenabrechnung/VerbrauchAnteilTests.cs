using Xunit;
using FakeItEasy;
using FluentAssertions;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class VerbrauchAnteilTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(0.1, 0.2)]
        [InlineData(2, 2)]
        [InlineData(100, 102)]
        [InlineData(102, 100)]
        public void VerbrauchAnteilTest(double delta, double anteil)
        {
            var kennnummer = A.Fake<string>();
            var typ = Zaehlertyp.Gas;

            var stub = new VerbrauchAnteil(kennnummer, typ, delta, anteil);

            stub.Should().BeOfType<VerbrauchAnteil>();
        }
    }
}