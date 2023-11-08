using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using Deeplex.Saverwalter.ModelTests;

namespace Deeplex.Saverwalter.PrintService.Tests
{
    public class DocxIntegrationTests
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
            abrechnung.SaveAsDocx(stream);

            // Assert
            stream.Length.Should().BeGreaterThan(1000);
        }
    }
}