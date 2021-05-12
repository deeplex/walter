using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.SecondPage
{
    public static partial class SecondPage
    {
        public static void Fill(Body body, Betriebskostenabrechnung b)
        {
            body.Append(
                Heading("Abrechnung der Nebenkosten"),
                ExplainUmlageschluessel(b),
                Heading("Erläuterungen zu einzelnen Betriebskostenarten"),
                ExplainKalteBetriebskosten(b),
                new Break() { Type = BreakValues.Page });
        }
    }
}
