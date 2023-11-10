using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.PrintService.Tests
{
    public class PdfIntegrationTests
    {
        [Fact]
        public void EverythingZeroTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);

            var abrechnung = new Betriebskostenabrechnung(
                ctx,
                vertrag,
                2021,
                new DateOnly(2021, 1, 1),
                new DateOnly(2021, 12, 31)
            );

            var stream = new MemoryStream();

            // Act
            abrechnung.SaveAsPdf(stream);

            // Assert
            stream.Length.Should().BeGreaterThan(1000);
        }
    }
}