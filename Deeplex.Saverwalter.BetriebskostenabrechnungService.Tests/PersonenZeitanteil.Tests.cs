using Deeplex.Saverwalter.Model;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService.Tests
{
    public class PersonenZeitanteilTests
    {
        [Theory]
        [InlineData(
            1, 1, 2020,
            31, 12, 2020,
            1, 1, 2019,
            1, 4, 2020, 0.25)]
        [InlineData(
            1, 1, 2020,
            1, 7, 2020,
            1, 1, 2019, 
            1, 4, 2020, 0.125)]
        [InlineData(
            1, 1, 2020,
            1, 7, 2020,
            1, 1, 2019,
            0, 2, 2020, 0)]
        [InlineData(
            1, 1, 2020,
            1, 7, 2020,
            1, 1, 2019,
            0, 0, 2020, 0)]
        public void PersonenZeitanteilTest(
            int bDay, int bMonth, int bYear,
            int eDay, int eMonth, int eYear,
            int vDay, int vMonth, int vYear,
            int personenzahl,
            int gesamtPersonenzahl,
            int year,
            double expectedAnteil)
        {
            var beginn = new DateOnly(bYear, bMonth, bDay);
            var ende = new DateOnly(eYear, eMonth, eDay);

            var vertragBeginn = new DateOnly(vYear, vMonth, vDay);
            var version = new VertragVersion(vertragBeginn, 1000, personenzahl);
            var vertrag = new Vertrag();
            vertrag.Versionen.Add(version);

            var zeitraum = new Zeitraum(year, vertrag);

            var output = new PersonenZeitanteil(beginn, ende, personenzahl, gesamtPersonenzahl, zeitraum);

            output.Anteil.Should().Be(expectedAnteil);

        }
    }
}
