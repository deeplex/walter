using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Linq;
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
                // TODO this is missing in viewmodel => move...
                body.Append(new Paragraph(
                    new Run(Font(),
                    new Text("Wir empfehlen Ihnen die monatliche Mietzahlung, um einen Betrag von " +
                    Euro(Anpassung) + " auf " + Euro(b.Gezahlt / 12 + Anpassung) + " anzupassen."))));
            }

            body.Append(new Paragraph(Font(),
                new ParagraphProperties(new Justification() { Val = JustificationValues.Both, }),
                new Run(Font(),
                new Text(b.GenerischerText()))),
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

        private static Paragraph PostalischerVermerk(Betriebskostenabrechnung b)
        {
            // TODO We have problems if there are more than 4 Mieter...

            var run = new Run(Font());
            int counter = 6;

            foreach (var m in b.Mieter)
            {
                run.Append(new Text(m.GetBriefAnrede()));
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

        private static Paragraph Ergebnis(Betriebskostenabrechnung b)
         => new Paragraph(Font(),
             new Run(Font(),
                 new Text(b.Gruss()),
                 new Break(),
                 new Text(b.ResultTxt()),
                 new TabChar()),
             new Run(Font(),
                 new RunProperties(
                     new Bold() { Val = OnOffValue.FromBoolean(true), },
                     new Underline() { Val = UnderlineValues.Single, }),
                 new Text(Euro(Math.Abs(b.Result))),
                 new Break()),
             new Run(Font(), new Text(b.RefundDemand())));

        private static Paragraph Betreff(Betriebskostenabrechnung b)
            => new Paragraph(Font(),
                new Run(Font(),
                    new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                    new Text(b.Title()),
                    new Break()),
                new Run(Font(),
                    new Text(b.Mieterliste()),
                    new Break(),
                    new Text(b.Mietobjekt()),
                    new Break(),
                    new Text("Abrechnungszeitraum: "),
                    new TabChar(),
                    new Text(b.Abrechnungszeitraum()),
                    new Break(),
                    new Text("Nutzungszeitraum: "),
                    new TabChar(),
                    new Text(b.Nutzungszeitraum())));
    }
}
