using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.PrintService.Tests
{
    public class UtilsTests
    {
        [Fact(Skip = "TODO = Replace with toDescription String")]
        public void AnschriftTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Theory]
        [InlineData(0, "0,00%")]
        [InlineData(0.1, "10,00%")]
        [InlineData(2, "200,00%")]
        [InlineData(100, "10.000,00%")]
        public void ProzentTest(double p, string s)
        {
            var stub = Utils.Prozent(p);
            stub.Should().Be(s);
        }


        [Theory]
        [InlineData(0, "0,00€")]
        [InlineData(0.1, "0,10€")]
        [InlineData(2, "2,00€")]
        [InlineData(100, "100,00€")]
        public void EuroTest(double p, string s)
        {
            var stub = Utils.Euro(p);
            stub.Should().Be(s);
        }


        [Theory]
        [InlineData(0, "m²", "0,00m²")]
        [InlineData(0.1, "kWh", "0,10kWh")]
        [InlineData(2, "whatever", "2,00whatever")]
        [InlineData(100, "test", "100,00test")]
        public void UnitTest(double p, string u, string s)
        {
            var stub = Utils.Unit(p, u);
            stub.Should().Be(s);
        }

        [Theory]
        [InlineData(0, "0,00°C")]
        [InlineData(0.1, "0,10°C")]
        [InlineData(2, "2,00°C")]
        [InlineData(100, "100,00°C")]
        public void CelsiusTest(double p, string s)
        {
            var stub = Utils.Celsius(p);
            stub.Should().Be(s);
        }

        [Theory]
        [InlineData(0, "0,00°C")]
        [InlineData(0.1, "0,00°C")] // Remember int!
        [InlineData(2, "2,00°C")]
        [InlineData(100, "100,00°C")]
        public void CelsiusTest2(int p, string s)
        {
            var stub = Utils.Celsius(p);
            stub.Should().Be(s);
        }

        [Theory]
        [InlineData(0, "0,00m²")]
        [InlineData(0.1, "0,10m²")]
        [InlineData(2, "2,00m²")]
        [InlineData(100, "100,00m²")]
        public void QuadratTest(double p, string s)
        {
            var stub = Utils.Quadrat(p);
            stub.Should().Be(s);
        }

        [Fact]
        public void DatumTest()
        {
            var mock = new DateOnly(1516, 3, 14);
            var stub = Utils.Datum(mock);
            stub.Should().Be("14.03.1516");
        }
    }
}