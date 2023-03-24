using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
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
        public void ToDescriptionStringTest()
        {
            for (int i = 0; i <= 32; i += 2)
            {
                ((Betriebskostentyp)i).ToDescriptionString().Should().NotBeNull();
            }
            (Betriebskostentyp.SchornsteinfegerarbeitenWarm).ToDescriptionString().Should().NotBeNull();
            (Betriebskostentyp.WasserversorgungWarm).ToDescriptionString().Should().NotBeNull();
            (Betriebskostentyp.Heizkosten).ToDescriptionString().Should().NotBeNull();
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