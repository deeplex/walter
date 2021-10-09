using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Table Abrechnungseinheit(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(
                new TableRow(
                    ContentHead("700", "Nutzeinheiten", JustificationValues.Center),
                    ContentHead("1050", "Wohnfläche", JustificationValues.Center),
                    ContentHead("950", "Nutzfläche", JustificationValues.Center),
                    ContentHead("600", "Bewohner", JustificationValues.Center),
                    ContentHead("1400", "Nutzungsintervall", JustificationValues.Center),
                    ContentHead("300", "Tage", JustificationValues.Center)));

            for (var i = 0; i < g.GesamtPersonenIntervall.Count(); ++i)
            {
                var z = g.GesamtPersonenIntervall[i];
                var f = z.Beginn.Date == b.Abrechnungsbeginn.Date;

                var timespan = ((z.Ende - z.Beginn).Days + 1).ToString();

                table.Append(new TableRow( // TODO check for duplicates...
                    ContentCell(f ? g.GesamtEinheiten.ToString() : "", JustificationValues.Center),
                    ContentCell(f ? Quadrat(g.GesamtWohnflaeche) : "", JustificationValues.Center),
                    ContentCell(f ? Quadrat(g.GesamtNutzflaeche) : "", JustificationValues.Center),
                    ContentCell(z.Personenzahl.ToString(), JustificationValues.Center),
                    ContentCell(Datum(z.Beginn) + " - " + Datum(z.Ende), JustificationValues.Center),
                    ContentCell(timespan + "/" + b.Abrechnungszeitspanne, JustificationValues.Center)));
            };

            return table;
        }
    }
}
