using Deeplex.Saverwalter.Model;
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
        private static Paragraph PostalischerVermerk(Betriebskostenabrechnung b)
        {
            // TODO We have problems if there are more than 4 Mieter...

            var run = new Run(Font());
            int counter = 6;

            foreach (var m in b.Mieter)
            {
                if (m is JuristischePerson j)
                {
                    run.Append(new Text(m.Bezeichnung));
                }
                else
                {
                    var Anrede = m.Anrede == Model.Anrede.Herr ? "Herrn " : m.Anrede == Model.Anrede.Frau ? "Frau " : "";
                    run.Append(new Text(Anrede + m.Bezeichnung));
                }
                run.Append(new Break());
                counter--;
            }
            var a = b.Mieter.First().Adresse;
            if (a != null)
            {
                run.Append(new Text(a.Strasse + " " + a.Hausnummer));
                run.Append(new Break());
                counter--;
                run.Append(new Text(a.Postleitzahl + " " + a.Stadt));
                run.Append(new Break());
                counter--;
            }

            run.Append(Enumerable.Range(0, counter).Select(_ => new Break()));

            return new Paragraph(Font(), run);
        }
    }
}
