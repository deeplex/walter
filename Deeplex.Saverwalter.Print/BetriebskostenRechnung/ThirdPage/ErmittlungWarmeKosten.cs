using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Table ErmittlungWarmeKosten(Betriebskostenabrechnung b, Rechnungsgruppe g, bool direkt = false)
        {
            var table = new Table(new TableWidth() { Width = "3000", Type = TableWidthUnitValues.Pct });
            foreach (var rechnung in g.Rechnungen.Where(r => (int)r.Typ % 2 == 1)) // Kalte Betriebskosten are equal / warme are odd
            {
                table.Append(
                    new TableRow(
                        ContentHead("2500", rechnung.Typ.ToDescriptionString()),
                        ContentHead("500", "Betrag", JustificationValues.Right)),
                    new TableRow(
                        ContentCell("Kosten für Brennstoffe"),
                        ContentCell(Euro(rechnung.Betrag), JustificationValues.Right)),
                    new TableRow(
                        ContentCell("Betriebskosten der Anlage (5% pauschal)"),
                        ContentCell(Euro(rechnung.Betrag * 0.05), JustificationValues.Right)));
            }
            table.Append(new TableRow(ContentHead("Gesamt"), ContentHead(Euro(g.GesamtBetragWarm), JustificationValues.Right)));

            return table;
        }
    }
}
