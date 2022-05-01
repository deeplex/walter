using Xunit;
using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using FluentAssertions;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class VerbrauchTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0.1)]
        [InlineData(2)]
        [InlineData(100)]
        [InlineData(102)]
        public void VerbrauchTest(double delta)
        {
            var btyp = Betriebskostentyp.Breitbandkabelanschluss;
            var ztyp = Zaehlertyp.Kaltwasser;

            var kennnummer = A.Fake<string>();
            var stub = new Verbrauch(btyp, kennnummer, ztyp, delta);

            stub.Should().BeOfType<VerbrauchAnteil>();
        }
    }
}