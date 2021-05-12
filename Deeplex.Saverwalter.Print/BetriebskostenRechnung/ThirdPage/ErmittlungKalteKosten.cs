using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Table ErmittlungKalteKosten(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color = "888888" })),
                new TableRow(
                    ContentHead("1600", "Kostenanteil", JustificationValues.Center),
                    ContentHead("450", "Schlüssel"),
                    ContentHead("1120", "Nutzungsintervall", JustificationValues.Center),
                    ContentHead("650", "Betrag", JustificationValues.Center),
                    ContentHead("550", "Ihr Anteil", JustificationValues.Right),
                    ContentHead("630", "Ihre Kosten", JustificationValues.Right)));

            TableRow kostenPunkt(Betriebskostenrechnung rechnung, string zeitraum, int Jahr, double anteil, bool f = true)
            {
                return new TableRow(
                    ContentCell(f ? rechnung.Typ.ToDescriptionString() : ""),
                    ContentCell(g.GesamtEinheiten == 1 ? "Direkt" : (f ? rechnung.Schluessel.ToDescriptionString() : "")),
                    ContentCell(zeitraum, JustificationValues.Center),
                    ContentCell(Euro(rechnung.Betrag), JustificationValues.Right), // TODO f ? bold : normal?
                    ContentCell(Prozent(anteil), JustificationValues.Right),
                    ContentCell(Euro(rechnung.Betrag * anteil), JustificationValues.Right));
            }

            foreach (var rechnung in g.Rechnungen.Where(r => (int)r.Typ % 2 == 0)) // Kalte Betriebskosten are equal / warme are odd
            {
                string zeitraum;
                switch (rechnung.Schluessel)
                {
                    case UmlageSchluessel.NachWohnflaeche:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        table.Append(kostenPunkt(rechnung, zeitraum, b.Jahr, g.WFZeitanteil));
                        break;
                    case UmlageSchluessel.NachNutzeinheit:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        table.Append(kostenPunkt(rechnung, zeitraum, b.Jahr, g.NEZeitanteil));
                        break;
                    case UmlageSchluessel.NachPersonenzahl:
                        var first = true;
                        foreach (var a in g.PersZeitanteil)
                        {
                            zeitraum = Datum(a.Beginn) + " - " + Datum(a.Ende);
                            table.Append(kostenPunkt(rechnung, zeitraum, b.Jahr, a.Anteil, first));
                            first = false;
                        }
                        break;
                    case UmlageSchluessel.NachVerbrauch:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        table.Append(kostenPunkt(rechnung, zeitraum, b.Jahr, g.VerbrauchAnteil[rechnung.Typ]));
                        break;
                    default:
                        break; // TODO or throw something...
                }
            }

            table.Append(new TableRow(
                ContentCell(""), ContentCell(""),
                ContentCell(""), ContentCell(""),
                ContentHead("Summe: ", JustificationValues.Center),
                ContentHead(Euro(g.BetragKalt), JustificationValues.Right)));

            return table;
        }
    }
}
