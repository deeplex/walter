using FakeItEasy;
using FluentAssertions;
using System;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class BetriebskostenabrechnungTests
    {
        [Theory(Skip = "How to fake SaverwalterContext and Vertrag?")]
        [InlineData(2020)]
        public void BetriebskostenabrechnungTest(int jahr)
        {
            var vfake = A.Fake<Vertrag>();
            var dfake = A.Fake<SaverwalterContext>();

            var beginn = DateTime.Parse("01/01/" + jahr.ToString());
            var ende = DateTime.Parse("12/31/" + jahr.ToString());

            var stub = new Betriebskostenabrechnung(
                dfake, vfake.rowid, jahr, beginn, ende);

            stub.Should().NotBeNull();
        }
    }
}