using Deeplex.Saverwalter.Model;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService.Tests
{
    public class AbrechnungseinheitTest
    {
        [Fact]
        public void AbrechnungseinheitIsValidForOneWohnung()
        {
            // Arrange
            var adresse = new Adresse("Teststraße", "Testhausnummer", "Testpostleitzahl", "Teststadt");
            var wohnungen = new List<Wohnung> { new("Testwohnung", 100, 200, 1) { Adresse = adresse } };
            var umlage = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachNutzeinheit);
            umlage.Wohnungen = wohnungen;
            var umlagen = new List<Umlage> { umlage };

            // Act
            var abrechnungseinheit = new Abrechnungseinheit(umlagen);

            // Assert
            abrechnungseinheit.GesamtWohnflaeche.Should().Be(100);
            abrechnungseinheit.GesamtNutzflaeche.Should().Be(200);
            abrechnungseinheit.GesamtEinheiten.Should().Be(1);
            abrechnungseinheit.Bezeichnung.Should().Be("Teststraße Testhausnummer, Testpostleitzahl Teststadt: Testwohnung");
        }

        [Fact]
        public void AbrechnungseinheitIsValidForManyWohnungen()
        {
            // Arrange
            var adresse = new Adresse("Teststraße", "Testhausnummer", "Testpostleitzahl", "Teststadt");
            var wohnungen = new List<Wohnung> {
                new("Testwohnung1", 100, 200, 1) { Adresse = adresse },
                new("Testwohnung2", 150, 300, 2) { Adresse = adresse },
            };
            var umlage = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachNutzeinheit);
            umlage.Wohnungen = wohnungen;
            var umlagen = new List<Umlage> { umlage };

            // Act
            var abrechnungseinheit = new Abrechnungseinheit(umlagen);

            // Assert
            abrechnungseinheit.GesamtWohnflaeche.Should().Be(250);
            abrechnungseinheit.GesamtNutzflaeche.Should().Be(500);
            abrechnungseinheit.GesamtEinheiten.Should().Be(3);
            abrechnungseinheit.Bezeichnung.Should().Be("Teststraße Testhausnummer, Testpostleitzahl Teststadt: Testwohnung1, Testwohnung2");
        }

        [Fact]
        public void AbrechnungseinheitOnlyCaresForUmlagenOfFirstWohnung()
        {
            // Arrange
            var adresse = new Adresse("Teststraße", "Testhausnummer", "Testpostleitzahl", "Teststadt");
            var wohnungen1 = new List<Wohnung>
            {
                new("Testwohnung1", 100, 200, 1) { Adresse = adresse }
            };
            var wohnungen2 = new List<Wohnung> {
                new("Testwohnung1", 100, 200, 1) { Adresse = adresse },
                new("Testwohnung2", 150, 300, 2) { Adresse = adresse },
            };
            var umlage1 = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachNutzeinheit);
            umlage1.Wohnungen = wohnungen1;
            var umlage2 = new Umlage(Betriebskostentyp.Breitbandkabelanschluss, Umlageschluessel.NachPersonenzahl);
            umlage2.Wohnungen = wohnungen2;
            var umlagen = new List<Umlage> { umlage1, umlage2 };

            // Act
            var abrechnungseinheit = new Abrechnungseinheit(umlagen);

            // Assert
            abrechnungseinheit.GesamtWohnflaeche.Should().Be(100);
            abrechnungseinheit.GesamtNutzflaeche.Should().Be(200);
            abrechnungseinheit.GesamtEinheiten.Should().Be(1);
            abrechnungseinheit.Bezeichnung.Should().Be("Teststraße Testhausnummer, Testpostleitzahl Teststadt: Testwohnung1");
        }
    }
}
