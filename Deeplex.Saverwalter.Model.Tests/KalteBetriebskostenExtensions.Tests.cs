using Deeplex.Saverwalter.Model;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.ModelTests
{
    public class KalteBetriebskostenExtensionsTests
    {
        [Fact]
        public void AsMinTest()
        {
            var mock = new DateTime();
            var stub = mock.AsMin();
            stub.Should().BeSameDateAs(DateTime.Now.AsUtcKind());
        }

        [Fact]
        public void AsMinTest2()
        {
            var mock = DateTime.Now;
            var stub = mock.AsMin();
            stub.Should().BeSameDateAs(DateTime.Now.AsUtcKind());
        }

        [Fact()]
        public void AsUtcKindTest()
        {
            var mock = DateTime.Now;
            var stub = mock.AsUtcKind();
            stub.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void ToUnitStringTest()
        {
            Zaehlertyp.Kaltwasser.ToUnitString().Should().Be("m³");
            Zaehlertyp.Warmwasser.ToUnitString().Should().Be("m³");
            Zaehlertyp.Strom.ToUnitString().Should().Be("kWh");
            Zaehlertyp.Gas.ToUnitString().Should().Be("kWh");
        }
    }
}
