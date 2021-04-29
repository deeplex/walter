using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deeplex.Saverwalter.Print.SecondPage
{
    public static partial class SecondPage
    {
        private static Paragraph ExplainKalteBetriebskosten(Betriebskostenabrechnung b)
        {
            var para = new Paragraph();

            foreach (var g in b.Gruppen)
            {
                foreach (var r in g.Rechnungen)
                {
                    if (r.Beschreibung != null && r.Beschreibung.Trim() != "")
                    {
                        para.Append(
                            new Run(
                                new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                                new Text(r.Typ.ToDescriptionString() + ": ") { Space = SpaceProcessingModeValues.Preserve }),
                            new Run(new Text(r.Beschreibung), new Break()));
                    }
                }
            }

            return para;
        }
    }
}
