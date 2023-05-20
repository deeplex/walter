using Deeplex.Saverwalter.Model;
using FakeItEasy;
using FluentAssertions;
using System;
using Xunit;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService.Tests
{
    public class UtilsTests
    {
        [Theory]
        [InlineData(1, "Frau Erika Mustermann")]
        [InlineData(0, "Herrn Erika Mustermann")]
        [InlineData(2, "Erika Mustermann")]
        public void GetBriefAnredeNatuerlichePersonTest(int anrede, string s)
        {
            var mock = new NatuerlichePerson("Mustermann")
            {
                Anrede = (Anrede)anrede,
                Vorname = "Erika",
            };

            mock.GetBriefAnrede().Should().Be(s);
        }

        [Theory]
        [InlineData("Muster AG")]
        public void GetBriefAnredeJuristischePersonTest(string s)
        {
            var mock = new JuristischePerson("Muster AG");

            mock.GetBriefAnrede().Should().Be(s);
        }

        [Fact(Skip = "TODO")]
        public void TitleTest()
        {
            // Arrange
            // Act
            // Assert
        }

        [Theory(Skip = "TODO")]
        [InlineData("Mieter: Erika Mustermann, Max Mustermann")]
        public void MieterlisteTest(string s)
        {
            var mock1 = new NatuerlichePerson("Mustermann")
            {
                Vorname = "Erika",
            };
            var mock2 = new NatuerlichePerson("Mustermann") { Vorname = "Max" };

            var fake = A.Fake<Betriebskostenabrechnung>();

            // Skip

            fake.Mieterliste().Should().Be(s);
        }


        [Fact(Skip = "TODO")]
        public void GrussTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void ResultTxtTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void RefundDemandTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void GenerischerTextTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void dirTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nWFTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nNFTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nNETest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nPZTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nVbTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void AnmerkungTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<Betriebskostenabrechnung>();
        }


        [Fact]
        public void GetKaltMieteTest()
        {
            // Arrange
            var vertrag = new Vertrag();
            var beginn = new DateOnly(2000, 1, 1);
            var version = new VertragVersion(beginn, 1000, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(version);
            var zeitraum = new Zeitraum(2020, vertrag);

            // Act
            var kaltMiete = Utils.GetKaltMiete(vertrag, zeitraum);

            // Assert
            kaltMiete.Should().Be(12000);
        }

        [Fact]
        public void GetKaltMieteTestOnVertragChange()
        {
            // Arrange
            var vertrag = new Vertrag();
            var beginn = new DateOnly(2000, 1, 1);
            var version1 = new VertragVersion(beginn.AddMonths(1), 1000, 1) { Vertrag = vertrag };
            var version2 = new VertragVersion(beginn.AddMonths(6), 1500, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(version1);
            vertrag.Versionen.Add(version2);
            var zeitraum = new Zeitraum(2000, vertrag);

            // Act
            var kaltMiete = Utils.GetKaltMiete(vertrag, zeitraum);

            // Assert
            kaltMiete.Should().Be(14000);
        }

        [Fact]
        public void GetKaltMieteTestOnVertragEnd()
        {
            // Arrange
            var vertrag = new Vertrag();
            var beginn = new DateOnly(2000, 1, 1);
            var version = new VertragVersion(beginn, 1000, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(version);
            vertrag.Ende = beginn.AddMonths(6);
            var zeitraum = new Zeitraum(2000, vertrag);

            // Act
            var kaltMiete = Utils.GetKaltMiete(vertrag, zeitraum);

            // Assert
            kaltMiete.Should().Be(7000);
        }


        [Fact]
        public void GetKaltMieteTestOnVertragEndBeforeStart()
        {
            // Arrange
            var vertrag = new Vertrag();
            var beginn = new DateOnly(2001, 1, 1);
            var version = new VertragVersion(beginn, 1000, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(version);
            vertrag.Ende = beginn.AddMonths(6);
            var zeitraum = new Zeitraum(2000, vertrag);

            // Act
            var kaltMiete = Utils.GetKaltMiete(vertrag, zeitraum);

            // Assert
            kaltMiete.Should().Be(0);
        }
    }
}