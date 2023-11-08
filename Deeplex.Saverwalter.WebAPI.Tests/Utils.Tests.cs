using Deeplex.Saverwalter.WebAPI.Helper;
using Xunit;
using FluentAssertions;
using Microsoft.VisualBasic;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UtilsTest
    {
        [Theory]
        [InlineData(0.0, "0,00€")]
        [InlineData(5.0, "5,00€")]
        [InlineData(0.01, "0,01€")]
        [InlineData(-100.0, "-100,00€")]
        [InlineData(-0.00001, "0,00€")]
        [InlineData(null, "0,00€")]
        public void EuroTestNullable(double? d, string expected)
        {
            var s = d.Euro();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(0.0, "0,00€")]
        [InlineData(5.0, "5,00€")]
        [InlineData(0.01, "0,01€")]
        [InlineData(-100.0, "-100,00€")]
        [InlineData(-0.00001, "0,00€")]
        public void EuroTest(double d, string expected)
        {
            var s = d.Euro();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(0.0, "0,00%")]
        [InlineData(5.0, "5,00%")]
        [InlineData(0.01, "0,01%")]
        [InlineData(-100.0, "-100,00%")]
        [InlineData(-0.00001, "0,00%")]
        public void ProzentTest(double d, string expected)
        {
            var s = d.Prozent();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(2021, 6, 31, "31.06.2021")]
        [InlineData(2020, 29, 2, "29.02.2020")]
        [InlineData(1999, 1, 1, "01.01.1999")]
        [InlineData(0, 1, 1, null)]
        public void ProzentDatumNull(int year, int month, int day, string? expected)
        {
            DateOnly? date = year != 0 ? new DateOnly(year, month, day) : null;
            var s = date.Datum();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(2021, 6, 31, "31.06.2021")]
        [InlineData(2020, 29, 2, "29.02.2020")]
        [InlineData(1999, 1, 1, "01.01.1999")]
        public void DatumTest(int year, int month, int day, string expected)
        {
            var date = new DateOnly(year, month, day);
            var s = date.Datum();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(2021, 6, 31, 12, 0, 0, "31.06.2021 12:00:00")]
        [InlineData(2020, 29, 2, 0, 0, 0, "29.02.2020 00:00:00")]
        [InlineData(1999, 1, 1, 17, 21, 48, "01.01.1999 17:21:48")]
        public void ZeitTest(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            string expected)
        {
            var date = new DateTime(year, month, day, hour, minute, second);
            var s = date.Zeit();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(2021, 6, 31, 12, 0, 0, "31.06.2021 12:00:00")]
        [InlineData(2020, 29, 2, 0, 0, 0, "29.02.2020 00:00:00")]
        [InlineData(1999, 1, 1, 17, 21, 48, "01.01.1999 17:21:48")]
        [InlineData(0, 1, 1, 0, 0, 0, null)]
        public void ZeitNullTest(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            string expected)
        {
            DateTime? date = year != 0 ? new DateTime(year, month, day, hour, minute, second) : null;
            var s = date.Zeit();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData("strasse", "plz", "hausnummer", "stadt")]
        [InlineData("strasse", "plz", "hausnummer", "stadt2")]
        [InlineData("strasse", "plz", "hausnummer", "stadt3")]
        [InlineData("strasse", "plz", "hausnummer", "stadt4")]
        public void GetAdresseTest(string strasse, string plz, string hausnummer, string stadt)
        {
            var adresseBase = new AdresseEntryBase()
            {
                Stadt = stadt,
                Strasse = strasse,
                Postleitzahl = plz,
                Hausnummer = hausnummer
            };

            var ctx = TestUtils.GetContext();
            ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt"));
            ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt2"));
            ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt3"));
            ctx.SaveChanges();

            var adresse = Utils.GetAdresse(adresseBase, ctx);

            adresse.Should().NotBe(null);
            adresse.Should().BeOfType<Adresse>();
            adresse!.Stadt.Should().Be(adresseBase.Stadt);
            adresse!.Strasse.Should().Be(adresseBase.Strasse);
            adresse!.Postleitzahl.Should().Be(adresseBase.Postleitzahl);
            adresse!.Hausnummer.Should().Be(adresseBase.Hausnummer);
        }

        [Theory]
        [InlineData("", "plz", "hausnummer", "stadt")]
        [InlineData("strasse", "", "hausnummer", "stadt")]
        [InlineData("strasse", "plz", "", "stadt")]
        [InlineData("strasse", "plz", "hausnummer", "")]
        [InlineData("strasse2", "plz", "hausnummer", "stadt")]
        public void GetAdresseTestNull(
            string strasse,
            string plz,
            string hausnummer,
            string stadt)
        {
            var adresseBase = new AdresseEntryBase()
            {
                Stadt = stadt,
                Strasse = strasse,
                Postleitzahl = plz,
                Hausnummer = hausnummer
            };

            var ctx = TestUtils.GetContext();
            ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt"));
            ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt2"));
            ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt3"));
            ctx.SaveChanges();

            var adresse = Utils.GetAdresse(adresseBase, ctx);

            adresse.Should().Be(null);
        }
    }
}