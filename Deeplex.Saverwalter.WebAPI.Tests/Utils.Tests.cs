using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Helper;
using FluentAssertions;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UtilsTest
    {
        [Theory]
        [InlineData(0.0, "0,00€")]
        [InlineData(5.0, "5,00€")]
        [InlineData(0.01, "0,01€")]
        [InlineData(-100.0, "-100,00€")]
        [InlineData(0.00001, "0,00€")]
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
        [InlineData(0.00001, "0,00€")]
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
        [InlineData(0.00001, "0,00%")]
        public void ProzentTest(double d, string expected)
        {
            var s = d.Prozent();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(2021, 6, 30, "30.06.2021")]
        [InlineData(2020, 2, 29, "29.02.2020")]
        [InlineData(1999, 1, 1, "01.01.1999")]
        [InlineData(0, 1, 1, null)]
        public void DatumNullTest(int year, int month, int day, string? expected)
        {
            DateOnly? date = year != 0 ? new DateOnly(year, month, day) : null;
            var s = date.Datum();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(2021, 6, 30, "30.06.2021")]
        [InlineData(2020, 2, 29, "29.02.2020")]
        [InlineData(1999, 1, 1, "01.01.1999")]
        public void DatumTest(int year, int month, int day, string expected)
        {
            var date = new DateOnly(year, month, day);
            var s = date.Datum();
            s.Should().Be(expected);
        }

        [Theory]
        [InlineData(2021, 6, 30, 12, 0, 0, "30.06.2021 12:00:00")]
        [InlineData(2020, 2, 29, 0, 0, 0, "29.02.2020 00:00:00")]
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
        [InlineData(2021, 6, 30, 12, 0, 0, "30.06.2021 12:00:00")]
        [InlineData(2020, 2, 29, 0, 0, 0, "29.02.2020 00:00:00")]
        [InlineData(1999, 1, 1, 17, 21, 48, "01.01.1999 17:21:48")]
        [InlineData(0, 1, 1, 0, 0, 0, null)]
        public void ZeitNullTest(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            string? expected)
        {
            DateTime? date = year != 0 ? new DateTime(year, month, day, hour, minute, second) : null;
            var s = date.Zeit();
            s.Should().Be(expected);
        }

        [Fact]
        public void GetAdresseTest1()
        {
            var adresseBase = new AdresseEntryBase()
            {
                Stadt = "stadtunique",
                Strasse = "strasse",
                Postleitzahl = "plz",
                Hausnummer = "hausnummer"
            };

            using (var ctx = TestUtils.GetContext())
            {
                // database is not empty on multiple runs...
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadtunique"));
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt2"));
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt3"));
                ctx.SaveChanges();

                var adresse = Utils.GetAdresse(adresseBase, ctx);

                adresse.Should().NotBe(null);
                adresse!.Stadt.Should().Be(adresseBase.Stadt);
                adresse!.Strasse.Should().Be(adresseBase.Strasse);
                adresse!.Postleitzahl.Should().Be(adresseBase.Postleitzahl);
                adresse!.Hausnummer.Should().Be(adresseBase.Hausnummer);
            }
        }

        [Fact]
        public void GetAdresseTest2()
        {
            var adresseBase = new AdresseEntryBase()
            {
                Stadt = "stadtveryuniquenotexistingyet",
                Strasse = "strasse",
                Postleitzahl = "plz",
                Hausnummer = "hausnummer"
            };

            using (var ctx = TestUtils.GetContext())
            {
                // database is not empty on multiple runs...
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt"));
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt2"));
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt3"));
                ctx.SaveChanges();

                var adresse = Utils.GetAdresse(adresseBase, ctx);

                adresse.Should().NotBe(null);
                adresse!.Stadt.Should().Be(adresseBase.Stadt);
                adresse!.Strasse.Should().Be(adresseBase.Strasse);
                adresse!.Postleitzahl.Should().Be(adresseBase.Postleitzahl);
                adresse!.Hausnummer.Should().Be(adresseBase.Hausnummer);
            }
        }

        [Theory]
        [InlineData("", "plz", "hausnummer", "stadt")]
        [InlineData("strasse", "", "hausnummer", "stadt")]
        [InlineData("strasse", "plz", "", "stadt")]
        [InlineData("strasse", "plz", "hausnummer", "")]
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

            using (var ctx = TestUtils.GetContext())
            {
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt"));
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt2"));
                ctx.Adressen.Add(new Adresse("strasse", "hausnummer", "plz", "stadt3"));
                ctx.SaveChanges();

                var adresse = Utils.GetAdresse(adresseBase, ctx);

                adresse.Should().Be(null);
            }
        }
    }
}
