using Deeplex.Saverwalter.Model;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void GetKaltMieteTest()
        {
            // Arrange
            var vertrag = new Vertrag();
            var beginn = new DateOnly(2000, 1, 1);
            var version = new VertragVersion(beginn, 1000, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(version);
            var zeitraum = new Zeitraum(2020, vertrag);

            // Act
            var kaltMiete = Utils.GetKaltMiete(vertrag, zeitraum);

            // Assert
            kaltMiete.Should().Be(12000);
        }

        [Fact]
        public void GetKaltMieteTestOnVertragChange()
        {
            // Arrange
            var vertrag = new Vertrag();
            var beginn = new DateOnly(2000, 1, 1);
            var version1 = new VertragVersion(beginn.AddMonths(1), 1000, 1) { Vertrag = vertrag };
            var version2 = new VertragVersion(beginn.AddMonths(6), 1500, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(version1);
            vertrag.Versionen.Add(version2);
            var zeitraum = new Zeitraum(2000, vertrag);

            // Act
            var kaltMiete = Utils.GetKaltMiete(vertrag, zeitraum);

            // Assert
            kaltMiete.Should().Be(14000);
        }

        [Fact]
        public void GetKaltMieteTestOnVertragEnd()
        {
            // Arrange
            var vertrag = new Vertrag();
            var beginn = new DateOnly(2000, 1, 1);
            var version = new VertragVersion(beginn, 1000, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(version);
            vertrag.Ende = beginn.AddMonths(6);
            var zeitraum = new Zeitraum(2000, vertrag);

            // Act
            var kaltMiete = Utils.GetKaltMiete(vertrag, zeitraum);

            // Assert
            kaltMiete.Should().Be(7000);
        }


        [Fact]
        public void GetKaltMieteTestOnVertragEndBeforeStart()
        {
            // Arrange
            var vertrag = new Vertrag();
            var beginn = new DateOnly(2001, 1, 1);
            var version = new VertragVersion(beginn, 1000, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(version);
            vertrag.Ende = beginn.AddMonths(6);
            var zeitraum = new Zeitraum(2000, vertrag);

            // Act
            var kaltMiete = Utils.GetKaltMiete(vertrag, zeitraum);

            // Assert
            kaltMiete.Should().Be(0);
        }
    }
}