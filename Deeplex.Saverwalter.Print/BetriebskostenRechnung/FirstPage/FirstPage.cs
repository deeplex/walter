using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
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
                Ergebnis(b));


            var Anpassung = -b.Result / 12;

            if (Anpassung > 0)
            {
                body.Append(new Paragraph(
                    new Run(Font(),
                    new Text("Wir empfehlen Ihnen die monatliche Mietzahlung, um einen Betrag von " +
                    Euro(Anpassung) + " auf " + Euro(b.Gezahlt / 12 + Anpassung) + " anzupassen."))));
            }

            body.Append(GenerischerText(b),
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
