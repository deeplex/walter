using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.FirstPage
{
    public static partial class FirstPage
    {
        private static Paragraph Betreff(Betriebskostenabrechnung b)
        {
            var Mieterliste = string.Join(", ", b.Mieter.Select(m => m.Bezeichnung));

            return new Paragraph(Font(),
                new Run(Font(),
                    new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                    new Text("Betriebskostenabrechnung " + b.Jahr.ToString()),
                    new Break()),
                new Run(Font(),
                    new Text("Mieter: " + Mieterliste),
                    new Break(),
                    new Text("Mietobjekt: " + b.Adresse.Strasse + " " +
                        b.Adresse.Hausnummer + ", " + b.Wohnung.Bezeichnung),
                    new Break(),
                    new Text("Abrechnungszeitraum: "),
                    new TabChar(),
                    new Text(Datum(b.Abrechnungsbeginn) + " - " + Datum(b.Abrechnungsende)),
                    new Break(),
                    new Text("Nutzungszeitraum: "),
                    new TabChar(),
                    new Text(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende))));
        }
    }
}
