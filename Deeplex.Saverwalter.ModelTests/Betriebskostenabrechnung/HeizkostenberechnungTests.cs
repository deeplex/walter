using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class HeizkostenberechnungTests
    {
        [Fact (Skip = "Can't fake Betriebskostenrechnung (Sealed)")]
        public void HeizkostenberechnungTest()
        {
            var fake1 = A.Fake<Betriebskostenrechnung>();
            var fake2 = A.Fake<IBetriebskostenabrechnung>();

            var stub = new Heizkostenberechnung(fake1, fake2);

            stub.Should().BeOfType<Heizkostenberechnung>();
        }
    }
}