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

        [Theory(Skip = "How to set Mieterliste to fake?")]
        [InlineData("Mieter: Erika Mustermann, Max Mustermann")]
        public void MieterlisteTest(string s)
        {
            var mock1 = new NatuerlichePerson("Mustermann")
            {
                Vorname = "Erika",
            };
            var mock2 = new NatuerlichePerson("Mustermann") { Vorname = "Max" };


            var fake = A.Fake<BetriebskostenabrechnungService.IBetriebskostenabrechnung>();

            // Skip

            fake.Mieterliste().Should().Be(s);
        }

        [Theory(Skip = "How to set Mietobjekt to fake?")]
        [InlineData("Mietobjekt: Musterstraße 3, 12345 Musterstadt - Musterwohnung")]
        public void MietobjektTest(string s)
        {
            var mockAdresse = new Adresse("Musterstraße", "3", "12345", "Musterstadt");
            var mock = new Wohnung("Musterwohnung", 100, 100, 1)
            {
                Adresse = mockAdresse,
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