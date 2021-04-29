using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Text;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Table GesamtErgebnis(Betriebskostenabrechnung b)
        {
            var table = new Table(
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color = "888888" })),
                new TableWidth() { Width = "2500", Type = TableWidthUnitValues.Pct },
                new TableRow(
                    ContentCell("Sie haben gezahlt:"),
                    ContentCell(Euro(b.Gezahlt), JustificationValues.Right)),
                new TableRow(
                    ContentCell("Abzüglich Ihrer Kaltmiete:"),
                    ContentCell("-" + Euro(b.KaltMiete), JustificationValues.Right)));

            if (b.Minderung > 0)
            {
                table.Append(new TableRow(
                    ContentCell("Verrechnung mit Mietminderung:"),
                    ContentCell("+" + Euro(b.KaltMinderung), JustificationValues.Right)));
            }

            var f = true;
            foreach (var gruppe in b.Gruppen)
            {
                if (gruppe.BetragKalt > 0)
                {
                    table.Append(new TableRow(
                        ContentCell(f ? "Abzüglich Ihrer Nebenkostenanteile:" : ""),
                        ContentCell("-" + Euro(gruppe.BetragKalt), JustificationValues.Right)));
                    f = false;
                }
            }

            foreach (var gruppe in b.Gruppen)
            {
                if (gruppe.BetragWarm > 0)
                {
                    table.Append(new TableRow(
                        ContentCell(f ? "Abzüglich Ihrer Nebenkostenanteile:" : ""),
                        ContentCell("-" + Euro(gruppe.BetragWarm), JustificationValues.Right)));
                }
                f = false;
            }

            if (b.Minderung > 0)
            {
                table.Append(new TableRow(
                    ContentCell("Verrechnung mit Mietminderung:"),
                    ContentCell("+" + Euro(b.NebenkostenMinderung), JustificationValues.Right)));
            }

            table.Append(new TableRow(
                ContentCell("Ergebnis:"),
                ContentHead(Euro(b.Result), JustificationValues.Right)));

            return table;
        }
    }
}
