// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.PrintService.Tests
{
    public class UtilsTests
    {
        [Theory]
        [InlineData("test", "test", "test", "test", "test test, test test")]
        [InlineData("Musterstraße", "7", "12345", "Musterstadt", "Musterstraße 7, 12345 Musterstadt")]
        public void AnschriftTest(
            string strasse,
            string hausnummer,
            string postleitzahl,
            string stadt,
            string output)
        {
            var adresse = new Adresse(strasse, hausnummer, postleitzahl, stadt);

            var anschrift = Utils.Anschrift(adresse);

            anschrift.Should().Be(output);
        }

        [Theory]
        [InlineData(0, "0,00%")]
        [InlineData(0.1, "10,00%")]
        [InlineData(2, "200,00%")]
        [InlineData(100, "10.000,00%")]
        public void ProzentTest(double p, string s)
        {
            var stub = Utils.Prozent(p);
            stub.Should().Be(s);
        }


        [Theory]
        [InlineData(0, "0,00€")]
        [InlineData(0.1, "0,10€")]
        [InlineData(2, "2,00€")]
        [InlineData(100, "100,00€")]
        public void EuroTest(double p, string s)
        {
            var stub = Utils.Euro(p);
            stub.Should().Be(s);
        }


        [Theory]
        [InlineData(0, "m²", "0,00m²")]
        [InlineData(0.1, "kWh", "0,10kWh")]
        [InlineData(2, "whatever", "2,00whatever")]
        [InlineData(100, "test", "100,00test")]
        public void UnitTest(double p, string u, string s)
        {
            var stub = Utils.Unit(p, u);
            stub.Should().Be(s);
        }

        [Theory]
        [InlineData(0, "0,00°C")]
        [InlineData(0.1, "0,10°C")]
        [InlineData(2, "2,00°C")]
        [InlineData(100, "100,00°C")]
        public void CelsiusTest(double p, string s)
        {
            var stub = Utils.Celsius(p);
            stub.Should().Be(s);
        }

        [Theory]
        [InlineData(0, "0,00°C")]
        [InlineData(0.1, "0,00°C")] // Remember int!
        [InlineData(2, "2,00°C")]
        [InlineData(100, "100,00°C")]
        public void CelsiusTest2(int p, string s)
        {
            var stub = Utils.Celsius(p);
            stub.Should().Be(s);
        }

        [Theory]
        [InlineData(0, "0,00m²")]
        [InlineData(0.1, "0,10m²")]
        [InlineData(2, "2,00m²")]
        [InlineData(100, "100,00m²")]
        public void QuadratTest(double p, string s)
        {
            var stub = Utils.Quadrat(p);
            stub.Should().Be(s);
        }

        [Theory]
        [InlineData(1516, 3, 14, "14.03.1516")]
        public void DatumTest(int year, int month, int day, string result)
        {
            var mock = new DateOnly(year, month, day);
            var stub = Utils.Datum(mock);
            stub.Should().Be(result);
        }

        [Theory]
        [InlineData("Test", null, Anrede.Herr, "Herrn Test", Rechtsform.natuerlich)]
        [InlineData("Test", "Mustername", Anrede.Herr, "Herrn Mustername Test", Rechtsform.natuerlich)]
        [InlineData("Test", null, Anrede.Frau, "Frau Test", Rechtsform.natuerlich)]
        [InlineData("Test", "Mustername", Anrede.Frau, "Frau Mustername Test", Rechtsform.natuerlich)]
        [InlineData("Test", null, Anrede.Keine, "Test", Rechtsform.natuerlich)]
        [InlineData("Test", "Mustername", Anrede.Keine, "Mustername Test", Rechtsform.natuerlich)]
        public void GetBriefAnredeTestNatuerlichePerson(string nachname, string? vorname, Anrede anrede, string result, Rechtsform rechtsform)
        {
            var person = new Kontakt(nachname, rechtsform)
            {
                Anrede = anrede,
                Vorname = vorname
            };

            var output = Utils.GetBriefAnrede(person);

            output.Should().Be(result);
        }

        [Theory]
        [InlineData("Test GmbH", "Test GmbH")]
        public void GetBriefAnredeTestJuristischePerson(string bezeichnung, string result)
        {
            var person = new Kontakt(bezeichnung, Rechtsform.gmbh) { };

            var output = Utils.GetBriefAnrede(person);

            output.Should().Be(result);
        }

        [Theory]
        [InlineData(2023, "Betriebskostenabrechnung 2023")]
        public void TitleTest(int jahr, string title)
        {
            var output = Utils.Title(jahr);

            output.Should().Be(title);
        }

        [Theory]
        [InlineData("test", "test", "test", "Mietobjekt: test test, test")]
        [InlineData("Musterstraße", "89", "Obergeschoss", "Mietobjekt: Musterstraße 89, Obergeschoss")]
        public void MietobjektTest(string strasse, string hausnummer, string bezeichnung, string result)
        {
            var adresse = new Adresse(strasse, hausnummer, "irrelevant", "irrelevant");
            var wohnung = new Wohnung(bezeichnung, 100, 100, 100, 1)
            {
                Adresse = adresse
            };

            var mietobjekt = Utils.Mietobjekt(wohnung);

            mietobjekt.Should().Be(result);
        }

        [Theory]
        [InlineData(2023, "01.01.2023 - 31.12.2023")]
        public void Abrechnungszeitraum(int jahr, string result)
        {
            var vertrag = new Vertrag();
            var zeitraum = new Zeitraum(jahr, vertrag);

            var abrechnungszeitraum = Utils.Abrechnungszeitraum(zeitraum);

            abrechnungszeitraum.Should().Be(result);
        }

        private const string resultTxtPositive
            = "wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. Die Abrechnung schließt mit einem Guthaben in Höhe von: ";
        private const string resultTxtNegative
            = "wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. Die Abrechnung schließt mit einer Nachforderung in Höhe von: ";
        [Theory]
        [InlineData(123.45, resultTxtPositive)]
        [InlineData(-123.45, resultTxtNegative)]
        public void ResultTxtTest(double input, string output)
        {
            var resultTxt = Utils.ResultTxt(input);
            resultTxt.Should().Be(output);
        }

        [Theory]
        [InlineData(20, Utils.RefundPositive)]
        [InlineData(-2, Utils.RefundNegative)]
        public void RefundDemandTest(double input, string output)
        {
            var refundDemand = Utils.RefundDemand(input);
            refundDemand.Should().Be(output);
        }
    }
}
