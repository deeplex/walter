using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class UtilsTests
    {
        [Theory]
        [InlineData(1, "Frau Erika Mustermann")]
        [InlineData(0, "Herrn Erika Mustermann")]
        [InlineData(2, "Erika Mustermann")]
        public void GetBriefAnredeNatuerlichePersonTest(int anrede, string s)
        {
            var mock = new NatuerlichePerson()
            {
                Anrede = (Anrede)anrede,
                Vorname = "Erika",
                Nachname = "Mustermann"
            };

            mock.GetBriefAnrede().Should().Be(s);
        }

        [Theory]
        [InlineData("Muster AG")]
        public void GetBriefAnredeJuristischePersonTest(string s)
        {
            var mock = new JuristischePerson()
            {
                Bezeichnung = "Muster AG"
            };

            mock.GetBriefAnrede().Should().Be(s);
        }

        [Theory(Skip = "Find a way to inject year")]
        [InlineData("Betriebskostenabrechnung 0", 0)]
        [InlineData("Betriebskostenabrechnung 1", 1)]
        [InlineData("Betriebskostenabrechnung 2022", 2022)]
        [InlineData("Betriebskostenabrechnung 31232", 31232)]
        public void TitleTest(string title, int year)
        {
            var fake = A.Fake<IBetriebskostenabrechnung>();
            fake.Title().Should().Be(title);
        }

        [Theory(Skip = "How to set Mieterliste to fake?")]
        [InlineData("Mieter: Erika Mustermann, Max Mustermann")]
        public void MieterlisteTest(string s)
        {
            var mock1 = new NatuerlichePerson()
            {
                Vorname = "Erika",
                Nachname = "Mustermann"
            };
            var mock2 = new NatuerlichePerson()
            {
                Vorname = "Max",
                Nachname = "Mustermann"
            };


            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();

            // Skip

            fake.Mieterliste().Should().Be(s);
        }

        [Theory(Skip = "How to set Mietobjekt to fake?")]
        [InlineData("Mietobjekt: Musterstraße 3, 12345 Musterstadt - Musterwohnung")]
        public void MietobjektTest(string s)
        {
            var mockAdresse = new Adresse()
            {
                Strasse = "Musterstraße",
                Hausnummer = "3",
                Postleitzahl = "12345",
                Stadt = "Musterstadt"
            };
            var mock = new Wohnung()
            {
                Adresse = mockAdresse,
                Bezeichnung = "Musterwohnung"
            };

            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();

            // Skip

            fake.Mietobjekt().Should().Be(s);
        }

        [Fact(Skip = "TODO")]
        public void AbrechnungszeitraumTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void NutzungszeitraumTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void GrussTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void ResultTxtTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void RefundDemandTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void GenerischerTextTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void dirTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nWFTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nNFTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nNETest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nPZTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void nVbTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }

        [Fact(Skip = "TODO")]
        public void AnmerkungTest()
        {
            Assert.True(false, "This test needs an implementation");
            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();
        }
    }
}