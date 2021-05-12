using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Deeplex.Saverwalter.Model.Betriebskostenabrechnung;
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
                var (Beginn, Ende, Personenzahl) = g.GesamtPersonenIntervall[i];
                var f = Beginn.Date == b.Abrechnungsbeginn.Date;

                var timespan = ((Ende - Beginn).Days + 1).ToString();

                table.Append(new TableRow( // TODO check for duplicates...
                    ContentCell(f ? g.GesamtEinheiten.ToString() : "", JustificationValues.Center),
                    ContentCell(f ? Quadrat(g.GesamtWohnflaeche) : "", JustificationValues.Center),
                    ContentCell(f ? Quadrat(g.GesamtNutzflaeche) : "", JustificationValues.Center),
                    ContentCell(Personenzahl.ToString(), JustificationValues.Center),
                    ContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center),
                    ContentCell(timespan + "/" + b.Abrechnungszeitspanne, JustificationValues.Center)));
            };

            return table;
        }
    }
}
