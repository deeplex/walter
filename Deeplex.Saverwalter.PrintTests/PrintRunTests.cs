using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.PrintService.Tests
{
    public class PrintRunTests
    {
        [Fact]
        public void PrintRunTest()
        {
            var stub = new PrintRun("test");
            stub.Should().NotBeNull();
        }
        [Fact]
        public void PrintRunTest2()
        {
            var stub = new PrintRun("test");
            stub.Should().NotBeNull();
        }
    }
}