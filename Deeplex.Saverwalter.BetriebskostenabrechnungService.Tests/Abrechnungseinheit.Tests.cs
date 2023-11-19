﻿using Deeplex.Saverwalter.Model;
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
            var wohnung = new Wohnung("Testwohnung", 100, 200, 1) { Adresse = adresse };
            var wohnungen = new List<Wohnung> { wohnung };
            var umlage = new Umlage(Umlageschluessel.NachNutzeinheit)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung"),
                Wohnungen = wohnungen
            };
            var umlagen = new List<Umlage> { umlage };

            var notes = new List<Note>();
            var vertrag = new Vertrag() { Wohnung = wohnung };
            var vertragVersion = new VertragVersion(new DateOnly(1970, 1, 1), 100, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(vertragVersion);
            var zeitraum = new Zeitraum(2022, vertrag);
            wohnung.Vertraege.Add(vertrag);

            // Act
            var abrechnungseinheit = new Abrechnungseinheit(umlagen, vertrag, zeitraum, notes);

            // Assert
            abrechnungseinheit.GesamtWohnflaeche.Should().Be(100);
            abrechnungseinheit.GesamtNutzflaeche.Should().Be(200);
            abrechnungseinheit.GesamtEinheiten.Should().Be(1);
            abrechnungseinheit.Bezeichnung
                .Should()
                    .Be("Teststraße Testhausnummer, Testpostleitzahl Teststadt: Testwohnung");
        }

        [Fact]
        public void AbrechnungseinheitIsValidForManyWohnungen()
        {
            // Arrange
            var adresse = new Adresse("Teststraße", "Testhausnummer", "Testpostleitzahl", "Teststadt");
            var wohnung = new Wohnung("Testwohnung1", 100, 200, 1) { Adresse = adresse };
            var wohnungen = new List<Wohnung> {
                wohnung,
                new("Testwohnung2", 150, 300, 2) { Adresse = adresse },
            };
            var umlage = new Umlage(Umlageschluessel.NachNutzeinheit)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung"),
                Wohnungen = wohnungen
            };
            var umlagen = new List<Umlage> { umlage };

            var notes = new List<Note>();
            var vertrag = new Vertrag() { Wohnung = wohnung };
            var vertragVersion = new VertragVersion(new DateOnly(1970, 1, 1), 100, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(vertragVersion);
            var zeitraum = new Zeitraum(2022, vertrag);
            wohnung.Vertraege.Add(vertrag);

            // Act
            var abrechnungseinheit = new Abrechnungseinheit(umlagen, vertrag, zeitraum, notes);

            // Assert
            abrechnungseinheit.GesamtWohnflaeche.Should().Be(250);
            abrechnungseinheit.GesamtNutzflaeche.Should().Be(500);
            abrechnungseinheit.GesamtEinheiten.Should().Be(3);
            abrechnungseinheit.Bezeichnung
                .Should()
                .Be("Teststraße Testhausnummer, Testpostleitzahl Teststadt: Testwohnung1, Testwohnung2");
        }

        [Fact]
        public void AbrechnungseinheitOnlyCaresForUmlagenOfFirstWohnung()
        {
            // Arrange
            var adresse = new Adresse("Teststraße", "Testhausnummer", "Testpostleitzahl", "Teststadt");
            var wohnung = new Wohnung("Testwohnung1", 100, 200, 1) { Adresse = adresse };
            var wohnungen1 = new List<Wohnung>
            {
                wohnung
            };
            var wohnungen2 = new List<Wohnung> {
                wohnung,
                new("Testwohnung2", 150, 300, 2) { Adresse = adresse },
            };
            var umlage1 = new Umlage(Umlageschluessel.NachNutzeinheit)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung"),
            };
            umlage1.Wohnungen = wohnungen1;
            var umlage2 = new Umlage(Umlageschluessel.NachPersonenzahl)
            {
                Typ = new Umlagetyp("Breitbandkabelanschluss")
            };
            umlage2.Wohnungen = wohnungen2;
            var umlagen = new List<Umlage> { umlage1, umlage2 };


            var notes = new List<Note>();
            var vertrag = new Vertrag() { Wohnung = wohnung };
            var vertragVersion = new VertragVersion(new DateOnly(1970, 1, 1), 100, 1) { Vertrag = vertrag };
            vertrag.Versionen.Add(vertragVersion);
            var zeitraum = new Zeitraum(2022, vertrag);
            wohnung.Vertraege.Add(vertrag);

            // Act
            var abrechnungseinheit = new Abrechnungseinheit(umlagen, vertrag, zeitraum, notes);

            // // Assert
            abrechnungseinheit.GesamtWohnflaeche.Should().Be(100);
            abrechnungseinheit.GesamtNutzflaeche.Should().Be(200);
            abrechnungseinheit.GesamtEinheiten.Should().Be(1);
            abrechnungseinheit.Bezeichnung.Should().Be("Teststraße Testhausnummer, Testpostleitzahl Teststadt: Testwohnung1");
        }
    }
}
