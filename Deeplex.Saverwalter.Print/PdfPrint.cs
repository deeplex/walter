using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Advanced;

namespace Deeplex.Saverwalter.PrintService
{

    public sealed class PdfPrint : IPrint<PdfDocument>
    {
        private static XUnit leftMargin = XUnit.FromMillimeter(25);
        private static XUnit rightMargin = XUnit.FromMillimeter(10);
        private static XUnit topMargin = XUnit.FromMillimeter(16.9);
        private static XUnit bottomMargin = XUnit.FromMillimeter(16.9);

        private static XUnit pageWidth = XUnit.FromMillimeter(210);
        private static XUnit pageHeight = XUnit.FromMillimeter(297);

        private static XUnit availableWidth = pageWidth - leftMargin - rightMargin;
        private static XUnit availableHeight = pageHeight - topMargin - bottomMargin;

        private XPoint currentPosition = new XPoint(leftMargin, topMargin);

        private static PdfDocument DinA4()
        {
            // TODO set A4 and stuff.
            var document = new PdfDocument();
            document.PageLayout = PdfPageLayout.OneColumn;
            var page = document.AddPage();

            return document;
        }

        public PdfDocument body { get; } = DinA4();

        public void Table(int[] widths, int[] justification, bool[] bold, bool[] underlined, string[][] cols)
        {
            //TableCell ContentCell(string str, JustificationValues value, BorderValues bordervalue = BorderValues.None)
            //    => new TableCell(
            //        new TableCellProperties(new BottomBorder() { Val = bordervalue, Size = 4 }),
            //        new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
            //        new Run(Font(), new Text(str))));

            //TableCell ContentCellWidth(string pct, string str, JustificationValues value, BorderValues bordervalue = BorderValues.None)
            //    => new TableCell(
            //        new TableCellProperties(new BottomBorder() { Val = bordervalue, Size = 4 }),
            //        new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
            //        new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
            //        new Run(Font(), new Text(str))));

            //TableCell ContentHeadWidth(string pct, string str, JustificationValues value, BorderValues bordervalue = BorderValues.None)
            //    => new TableCell(
            //        new TableCellProperties(new BottomBorder() { Val = bordervalue, Size = 4 }),
            //        new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
            //        new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
            //        new Run(Font(), Bold(), new Text(str))));

            //TableCell ContentHead(string str, JustificationValues value, BorderValues bordervalue = BorderValues.None)
            //    => new TableCell(
            //        new TableCellProperties(new BottomBorder() { Val = bordervalue, Size = 4 }),
            //        new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
            //        new Run(Font(), Bold(), new Text(str))));

            if (widths.Count() != cols.Count() || widths.Count() != justification.Count())
            {
                throw new Exception("Table parameters are not properly specified");
            }

            var j = justification.Select(w => w == 0 ?
                JustificationValues.Left : w == 1 ?
                JustificationValues.Center :
                JustificationValues.Right).ToList();

            var table = new Table(new TableWidth() { Width = (widths.Sum() * 50).ToString(), Type = TableWidthUnitValues.Pct });
            var headrow = new TableRow();

            var heads = cols.Select(w => w.First()).ToList();
            for (var i = 0; i < widths.Count(); ++i)
            {
                if (bold[0])
                {
                    //headrow.Append(ContentHeadWidth((widths[i] * 50).ToString(), heads[i] ?? "", j[i], underlined[0] ? BorderValues.Single : BorderValues.None));
                }
                else
                {
                    //headrow.Append(ContentCellWidth((widths[i] * 50).ToString(), heads[i] ?? "", j[i], underlined[0] ? BorderValues.Single : BorderValues.None));
                }
            }
            table.Append(headrow);

            var max = cols.Max(w => w.Length);

            for (var i = 1; i < max; ++i)
            {
                var cellrow = new TableRow();
                var row = cols.Select(w => w.Skip(i).FirstOrDefault()).ToList();
                for (var k = 0; k < row.Count; ++k)
                {
                    if (bold[i])
                    {
                        //cellrow.Append(ContentHead(row[k] ?? "", j[k], underlined[i] ? BorderValues.Single : BorderValues.None));
                    }
                    else
                    {
                        //cellrow.Append(ContentCell(row[k] ?? "", j[k], underlined[i] ? BorderValues.Single : BorderValues.None));
                    }
                }
                table.Append(cellrow);
            }

            //body.Append(table);
        }

        public void Paragraph(PrintRun[] runs)
        {
            for (var i = 0; i < runs.Length; ++i)
            {
                var run = runs[i];

                var text = run.Text;
                if (run.Tab)
                {
                    text += "\t";
                }

                if (run.Bold && run.Underlined)
                {
                    drawTextAndUpdatePosition(text, BoldUnderline);
                }
                else if (run.Bold)
                {
                    drawTextAndUpdatePosition(text, Bold);
                }
                else if (run.Underlined)
                {
                    drawTextAndUpdatePosition(text, Underline);
                }
                else
                {
                    drawTextAndUpdatePosition(text, Regular);
                }
                
                if (!run.NoBreak)
                {
                    Break();
                }
            }
            currentPosition.Y += spacing + 1;
            currentPosition.X = leftMargin;
        }

        public void Text(string text)
        {
            drawTextAndUpdatePosition(text, Regular);
            Break();
        }
        public void PageBreak()
        {
            body.AddPage();
            currentPosition.X = leftMargin;
            currentPosition.Y = topMargin;
        }
        public void EqHeizkostenV9_2(Rechnungsgruppe gruppe)
        {
            //RunProperties rp() => new RunProperties(new RunFonts() { Ascii = "Cambria Math", HighAnsi = "Cambria Math" });
            //DocumentFormat.OpenXml.Math.Run t(string str) => new DocumentFormat.OpenXml.Math.Run(rp(), new DocumentFormat.OpenXml.Math.Text(str));
            ////Run r(string str) => new Run(Font(), new Text(str) { Space = SpaceProcessingModeValues.Preserve });

            //DocumentFormat.OpenXml.Math.OfficeMath om(string str) => new DocumentFormat.OpenXml.Math.OfficeMath(t(str));
            //DocumentFormat.OpenXml.Math.OfficeMath omtw() => new DocumentFormat.OpenXml.Math.OfficeMath(tw());

            //DocumentFormat.OpenXml.Math.ParagraphProperties justifyLeft
            //    () => new DocumentFormat.OpenXml.Math.ParagraphProperties(
            //        new DocumentFormat.OpenXml.Math.Justification()
            //        { Val = DocumentFormat.OpenXml.Math.JustificationValues.Left });

            //DocumentFormat.OpenXml.Math.Base tw()
            //    => new DocumentFormat.OpenXml.Math.Base(
            //            new DocumentFormat.OpenXml.Math.Subscript(
            //            new DocumentFormat.OpenXml.Math.SubscriptProperties(
            //            new DocumentFormat.OpenXml.Math.ControlProperties(rp())),
            //            new DocumentFormat.OpenXml.Math.Base(t("t")),
            //            new DocumentFormat.OpenXml.Math.SubArgument(t("w"))));

            //DocumentFormat.OpenXml.Math.Fraction frac(string num, string den)
            //    => new DocumentFormat.OpenXml.Math.Fraction(new DocumentFormat.OpenXml.Math.FractionProperties(
            //        new DocumentFormat.OpenXml.Math.ControlProperties(rp())),
            //        new DocumentFormat.OpenXml.Math.Numerator(t(num)),
            //        new DocumentFormat.OpenXml.Math.Denominator(t(den)));

            //DocumentFormat.OpenXml.Math.Fraction units() => frac("kWh", "m³ x K");

            // TODO
            //var p = new Paragraph(Font(), new Run(Font(), new Break(), new Text("Davon der Warmwasseranteil nach HeizkostenV §9(2):"), new Break(), new Break()));

            //foreach (var hk in gruppe.Heizkosten)
            //{
            //    p.Append(new DocumentFormat.OpenXml.Math.Paragraph(justifyLeft(),
            //        new DocumentFormat.OpenXml.Math.OfficeMath(
            //        t("2,5 ×"), frac("V", "Q"), t(" × ("), tw(), t("-10°C)"), units(), t(" ⟹ "),
            //        t("2,5 ×"), frac(Utils.Unit(hk.V, "m³"), Utils.Unit(hk.Q, "kWh")), t(" × ("), t(Utils.Celsius((int)hk.tw)), t("-10°C)"), units(), t(" = "), t(Utils.Prozent(hk.Para9_2)))));
            //}

            //p.Append(new Break(), new Break(),
            //    new Run(Font(), r("Wobei "), om("V"), r(" die Menge des Warmwassers, die im Abrechnungszeitraum gemessen wurde, "),
            //    om("Q"), r(" die gemessene Wärmemenge am Allgemeinzähler und "), omtw(), r("die geschätzte mittlere Temperatur des Warmwassers darstellt.")));

            // TODO
            //body.Append(p);
        }
        public void Break()
        {
            currentPosition.Y += fontSize;
            currentPosition.X = leftMargin;
        }
        public void Heading(string text)
        {
            drawTextAndUpdatePosition(text, BoldItalic);
            Break();
        }
        public void SubHeading(string text)
        {
            drawTextAndUpdatePosition(text, Bold);
            Break();
        }

        private void drawTextAndUpdatePosition(string text, XFont font)
        {
            var pdfPage = body.Pages[^1];
            using (var gfx = XGraphics.FromPdfPage(pdfPage))
            {
                var formatter = new XTextFormatter(gfx);
                var stringSize = gfx.MeasureString(text, font);

                var neededHeight = stringSize.Height + (stringSize.Height * 1.5 * Math.Floor(stringSize.Width / availableWidth));
                var neededRect = new XPoint(availableWidth, currentPosition.Y + neededHeight + 1);
                var rect = new XRect(currentPosition, neededRect);

                gfx.DrawRectangle(XBrushes.Gray, rect);
                formatter.DrawString(text, font, XBrushes.Black, rect);

                currentPosition.Y += neededHeight - stringSize.Height;
                currentPosition.X += stringSize.Width;
            }
        }

        private static string font = "Times New Roman";
        private static int fontSize = 11;
        private static int spacing = 8;

        private static XFont Regular = new(font, fontSize, XFontStyle.Regular);
        private static XFont Bold = new(font, fontSize, XFontStyle.Bold);
        private static XFont BoldUnderline = new(font, fontSize, XFontStyle.Bold | XFontStyle.Underline);
        private static XFont BoldItalic = new(font, fontSize, XFontStyle.BoldItalic);
        private static XFont Underline = new(font, fontSize, XFontStyle.Underline);

        public static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
    }
}
