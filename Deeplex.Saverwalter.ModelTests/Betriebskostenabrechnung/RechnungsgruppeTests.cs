using FakeItEasy;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class RechnungsgruppeTests
    {
        [Fact (Skip = "How to fake Betriebskostenrechnung")]
        public void RechnungsgruppeTest()
        {
            var fake1 = A.Fake<IBetriebskostenabrechnung>();
            var fake2 = A.Fake<List<Betriebskostenrechnung>>();
            var stub = new Rechnungsgruppe(fake1, fake2);

            stub.Should().BeOfType<Rechnungsgruppe>();
        }
    }
}