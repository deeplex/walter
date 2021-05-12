using Deeplex.Saverwalter.Model.ErhaltungsaufwendungListe;
using DocumentFormat.OpenXml.Wordprocessing;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print
{
    public static class Erhaltungsaufwendung
    {
        public static void ErhaltungsaufwendungWohnungBody(Body body, ErhaltungsaufwendungWohnung e)
        {
            body.Append(Heading(Anschrift(e.Wohnung.Adresse) + ", " + e.Wohnung.Bezeichnung));

            var table = new Table(
                new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color = "888888" })),
                new TableRow(
                    ContentHead("2000", "Aussteller", JustificationValues.Center),
                    ContentHead("750", "Datum"),
                    ContentHead("1550", "Bezeichnung"),
                    ContentHead("650", "Betrag", JustificationValues.Right)));

            foreach (var a in e.Liste)
            {
                table.Append(new TableRow(
                    ContentCell(a.Aussteller.Bezeichnung),
                    ContentCell(a.Datum.ToString("dd.MM.yyyy")),
                    ContentCell(a.Bezeichnung),
                    ContentCell(Euro(a.Betrag), JustificationValues.Right)));
            }
            table.Append(new TableRow(
                ContentCell(""),
                ContentCell(""),
                ContentCell("Summe:", JustificationValues.Center),
                ContentCell(Euro(e.Summe), JustificationValues.Right)));

            body.Append(table);
        }
    }
}
