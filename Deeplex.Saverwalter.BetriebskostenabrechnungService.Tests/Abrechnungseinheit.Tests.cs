using Deeplex.Saverwalter.Model;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService.Tests
{
    public class AbrechnungseinheitTest
    {
        [Fact]
        public void AbrechnungseinheitIsValid()
        {
            // Arrange
            var umlagen = A.Fake<List<Umlage>>();
            var abrechnungseinheit = new Abrechnungseinheit(umlagen);
            // Act

            // Assert
            abrechnungseinheit.Bezeichnung.Should().Be("Whatever");
        }
    }
}
