using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BetriebskostenabrechnungHandlerTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var handler = new BetriebskostenabrechnungHandler(ctx);

            var result = handler.Get(vertrag.VertragId, 2021);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetWordDocumentTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var handler = new BetriebskostenabrechnungHandler(ctx);

            var result = handler.GetWordDocument(vertrag.VertragId, 2021);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetPdfDocumentTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var handler = new BetriebskostenabrechnungHandler(ctx);

            var result = handler.GetPdfDocument(vertrag.VertragId, 2021);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
