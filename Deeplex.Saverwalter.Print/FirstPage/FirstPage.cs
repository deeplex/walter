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
        public static void Fill(Body body, Betriebskostenabrechnung b)
        {
            body.Append(
                AnschriftVermieter(b),
                EmptyRows(3),
                PostalischerVermerk(b),
                PrintDate(),
                Betreff(b),
                Ergebnis(b),
                GenerischerText(),
                new Break() { Type = BreakValues.Page });
        }

        private static Paragraph EmptyRows(int len)
        {
            var p = new Paragraph(Font());
            var r = new Run(Font());
            p.Append(r);
            for (var i = 0; i < len; ++i)
            {
                r.Append(new Break());
            }
            return p;
        }

        private static Paragraph PrintDate()
        {
            return new Paragraph(new ParagraphProperties(new Justification
            {
                Val = JustificationValues.Right,
            }),
                new Run(Font(), new Text(Datum(DateTime.UtcNow.Date))));
        }
    }
}
