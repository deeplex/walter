using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        public static void Fill(Body body, Betriebskostenabrechnung b)
        {
            body.Append(
                SubHeading("Angaben zu Ihrer Einheit:"),
                Abrechnungswohnung(b, b.Gruppen.FirstOrDefault()),
                new Paragraph(NoSpace()),
                Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)"));
            // MietHeader(b), TODO Make this in a heading

            foreach (var gruppe in b.Gruppen)
            {
                body.Append(
                    SubHeading("Angaben zur Abrechnungseinheit:", true),
                    Abrechnungsgruppe(b, gruppe));

                if (gruppe.GesamtEinheiten > 1)
                {
                        body.Append(
                            Abrechnungseinheit(b, gruppe),
                            new Paragraph(NoSpace()));
                }
                body.Append(
                    ErmittlungKalteEinheiten(b, gruppe),
                    new Paragraph(NoSpace()),
                    ErmittlungKalteKosten(b, gruppe));
            }

            // TODO apply sleeker format like for kalte Kosten for warme Kosten.
            if (b.Gruppen.Any(g => g.GesamtBetragWarm != 0 && g.BetragWarm != 0))
            {
                
                body.Append(
                    new Break() { Type = BreakValues.Page }, // forth page...
                    Heading("Abrechnung der Nebenkosten (warme Nebenkosten)"));
                foreach (var gruppe in b.Gruppen)
                {
                    if (gruppe.GesamtBetragWarm > 0)
                    {
                        if (gruppe.GesamtEinheiten == 1)
                        {
                            body.Append(
                                SubHeading("Direkt zugeordnet:"),
                                ErmittlungWarmeKosten(b, gruppe, true));
                        }
                        else
                        {
                            body.Append(
                                SubHeading("Angaben zur Abrechnungseinheit:", true),
                                Abrechnungsgruppe(b, gruppe),
                                Abrechnungseinheit(b, gruppe),
                                new Paragraph(NoSpace()),
                                ErmittlungWarmeKosten(b, gruppe),
                                EqHeizkostenV9_2(b, gruppe), // Only if §9(2) applies
                                ErmittlungWarmeEinheiten(b, gruppe),
                                SubHeading("Ermittlung der warmen Betriebskosten", true),
                                ErmittlungWarmanteil(b, gruppe));
                            //ErmittlungAnteilWarmWasser(b, gruppe));
                        }
                    }
                }
            }

            body.Append(
                new Paragraph(),
                Heading("Gesamtergebnis der Abrechnung"),
                GesamtErgebnis(b));            
        }
    }
}
