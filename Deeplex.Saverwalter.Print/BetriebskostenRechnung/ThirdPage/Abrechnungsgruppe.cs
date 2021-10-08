﻿using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Paragraph Abrechnungsgruppe(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var p = new Paragraph(Font());

            var adressen = g.Rechnungen.First().Gruppen.Select(w => w.Wohnung).GroupBy(w => w.Adresse);

            foreach (var adr in adressen)
            {
                var a = adr.Key;
                var ret = Anschrift(a);

                if (adr.Count() != a.Wohnungen.Count)
                {
                    ret += ": " + string.Join(", ", adr.Select(w => w.Bezeichnung));
                }
                else
                {
                    ret += " (gesamt)";
                }

                p.Append(new Run(Font(), new Text(ret)));
                if (a != adressen.Last().Key)
                {
                    p.Append(new Break());
                }
            }

            return p;
        }
    }
}
