using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Table Abrechnungswohnung(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(new TableRow(
                ContentHead("700", "Nutzeinheiten", JustificationValues.Center),
                ContentHead("1050", "Wohnfläche", JustificationValues.Center),
                ContentHead("950", "Nutzfläche", JustificationValues.Center),
                ContentHead("600", "Bewohner", JustificationValues.Center),
                ContentHead("1400", "Nutzungsintervall", JustificationValues.Center),
                ContentHead("300", "Tage", JustificationValues.Center)));

            if (g == null) return table; // If Gruppen is empty...

            for (var i = 0; i < g.PersonenIntervall.Count(); ++i)
            {
                var z = g.PersonenIntervall[i];
                var f = z.Beginn.Date == b.Nutzungsbeginn.Date;

                table.Append(new TableRow(
                    ContentCell(f ? 1.ToString() : "", JustificationValues.Center), // TODO  ... 1 ? hmm...
                    ContentCell(f ? Quadrat(b.Wohnung.Wohnflaeche) : "", JustificationValues.Center),
                    ContentCell(f ? Quadrat(b.Wohnung.Nutzflaeche) : "", JustificationValues.Center),
                    ContentCell(z.Personenzahl.ToString(), JustificationValues.Center),
                    ContentCell(Datum(z.Beginn) + " - " + Datum(z.Ende), JustificationValues.Center),
                    ContentCell(b.Nutzungszeitspanne + "/" + b.Abrechnungszeitspanne, JustificationValues.Center)));
            };

            return table;
        }
    }
}
