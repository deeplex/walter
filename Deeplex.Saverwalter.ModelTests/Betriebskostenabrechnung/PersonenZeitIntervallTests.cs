using FakeItEasy;
using FluentAssertions;
using System;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class PersonenZeitIntervallTests
    {
        [Theory(Skip = "How to fake Rechnungsgruppe")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void PersonenZeitIntervallTest(int p)
        {
            var mockB = DateTime.Now.AddDays(-365);
            var mockE = DateTime.Now;

            var fake = A.Fake<Rechnungsgruppe>();
            var stub = new PersonenZeitIntervall((mockB, mockE, p), fake);

            stub.Should().NotBeNull();
        }
    }
}