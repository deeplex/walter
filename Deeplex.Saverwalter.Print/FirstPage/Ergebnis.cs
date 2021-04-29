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
        private static Paragraph Ergebnis(Betriebskostenabrechnung b)
        {
            var gruss = b.Mieter.Aggregate("", (r, m) =>
            {
                if (m is NatuerlichePerson n)
                {
                    return (n.Anrede == Anrede.Herr ? "sehr geehrter Herr " :
                        n.Anrede == Anrede.Frau ? "sehr geehrte Frau " :
                        n.Vorname) + n.Nachname + ", ";
                }
                else
                {
                    return "Sehr geehrte Damen und Herren, ";
                }
            });


            // Capitalize first letter...
            var Gruss = gruss.Remove(1).ToUpper() + gruss.Substring(1);

            var resultTxt1 = "Die Abrechnung schließt mit " +
                (b.Result > 0 ? "einem Guthaben" : "einer Nachforderung") +
                " in Höhe von: ";

            var refund = new Run(Font(),
                new Text("Dieser Betrag wird über die von Ihnen angegebene Bankverbindung erstattet."));

            var demand = new Run(Font(), new Text("Bitte überweisen Sie diesen Betrag auf das Ihnen bekannte Konto."));

            return new Paragraph(Font(),
                new Run(Font(),
                    new Text(Gruss),
                    new Break(),
                    new Text("wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. " +
                        resultTxt1),
                    new TabChar()),
                new Run(Font(),
                    new RunProperties(
                        new Bold() { Val = OnOffValue.FromBoolean(true), },
                        new Underline() { Val = UnderlineValues.Single, }),
                    new Text(Euro(Math.Abs(b.Result))),
                    new Break()),
                b.Result > 0 ? refund : demand);
        }
    }
}
