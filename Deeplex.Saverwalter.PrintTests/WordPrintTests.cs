using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.PrintService.Tests
{
    public class WordPrintTests
    {
        [Fact(Skip = "TODO")]
        public void TableTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO")]
        public void ParagraphTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO")]
        public void TextTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO")]
        public void PageBreakTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO")]
        public void EqHeizkostenV9_2Test()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO")]
        public void BreakTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO")]
        public void HeadingTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO")]
        public void SubHeadingTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "Check for properties")]
        public void FontTest()
        {
            var stub = DocxPrint.Font();
            stub.Should().NotBeNull();
        }

        [Fact]
        public void BoldTest()
        {
            var stub = DocxPrint.Bold();
            stub.Should().NotBeNull();
        }

        [Fact]
        public void NoSpaceTest()
        {
            var stub = DocxPrint.NoSpace();
            stub.Should().NotBeNull();
        }
    }
}