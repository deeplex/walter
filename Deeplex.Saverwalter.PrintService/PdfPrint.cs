using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace Deeplex.Saverwalter.PrintService
{

    public sealed class PdfPrint : IPrint<Document>
    {
        private static double leftMargin = 25;
        private static double rightMargin = 10;
        private static double topMargin = 16.9;
        private static double bottomMargin = 16.9;

        private static double pageWidth = 210;
        private static double pageHeight = 297;

        private static Document DinA4()
        {
            // TODO set A4 and stuff.
            var document = new Document();
            document.AddSection();

            return document;
        }

        public Document body { get; } = DinA4();

        public void Table(int[] widths, int[] justification, bool[] bold, bool[] underlined, string[][] cols)
        {
            if (widths.Count() != cols.Count() || widths.Count() != justification.Count())
            {
                throw new Exception("Table parameters are not properly specified");
            }

            var table = new Table();
            foreach (var width in widths)
            {
                var width_in_mm = (pageWidth - leftMargin - rightMargin) * ((double)width / 100);
                table.AddColumn(Unit.FromMillimeter(width_in_mm));
            }

            for (var i = 0; i < cols.Count(); ++i)
            {
                for (var j = 0; j < cols[i].Count(); ++j)
                {
                    if (i == 0)
                    {
                        table.AddRow();
                    }
                    var row = table.Rows[j];
                    var cell = row.Cells[i];
                    if (underlined[j])
                    {
                        cell.Borders.Bottom = new Border { Style = BorderStyle.Single, Width = 0.25 };
                    }

                    cell.AddParagraph(cols[i][j]);
                    cell.Format.Font.Size = fontSize;
                    cell.Format.Font.Name = font;
                    cell.Format.Alignment = (ParagraphAlignment)justification[i];
                    cell.Format.Font.Bold = bold[j];
                }
            }

            var section = body.LastSection;
            section.Add(table);
        }

        public void Paragraph(PrintRun[] runs)
        {
            var section = body.LastSection;
            var para = new Paragraph();

            for (var i = 0; i < runs.Length; ++i)
            {
                var run = runs[i];

                var format = new FormattedText();
                format.Font.Size = fontSize;
                format.Font.Name = font;
                format.AddText(run.Text);

                if (run.Bold)
                {
                    format.Font.Bold = true;
                }
                if (run.Underlined)
                {
                    format.Font.Underline = Underline.Single;
                }

                para.Add(format);

                if (run.Tab)
                {
                    para.AddTab();
                }

                if (!run.NoBreak && i != runs.Length - 1)
                {
                    para.AddLineBreak();
                }
            }
            para.Format.SpaceAfter = Unit.FromPoint(spacing);

            section.Add(para);
        }

        public void Text(string text)
        {
            var section = body.LastSection;
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = fontSize;
            paragraph.Format.Font.Name = font;
            paragraph.Format.SpaceAfter = Unit.FromPoint(spacing);

            paragraph.AddFormattedText(text);
        }
        public void PageBreak()
        {
            var section = body.AddSection();
            section.AddPageBreak();
        }
        public void EqHeizkostenV9_2(IAbrechnungseinheit gruppe)
        {
            Text("Davon der Warmwasseranteil nach HeizkostenV §9(2):");

            foreach (var hk in gruppe.Heizkosten)
            {
                // TODO implement...
                Text(Utils.Prozent(hk.Para9_2));
            }
        }

        private string frac(string num, string den)
        {
            return $"{num} / {den}";
        }

        public void Break()
        {
            var section = body.LastSection;
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = fontSize;
            paragraph.Format.Font.Name = font;
        }

        public void Heading(string text)
        {
            var section = body.LastSection;
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = fontSize;
            paragraph.Format.Font.Name = font;
            paragraph.Format.SpaceAfter = Unit.FromPoint(spacing);

            paragraph.AddFormattedText(text, TextFormat.Bold | TextFormat.Italic);
        }

        public void SubHeading(string text)
        {
            var section = body.LastSection;
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = fontSize;
            paragraph.Format.Font.Name = font;
            paragraph.Format.SpaceAfter = Unit.FromPoint(spacing);

            paragraph.AddFormattedText(text, TextFormat.Bold);
        }

        private static string font = "Times New Roman";
        private static Unit fontSize = 11;
        private static Unit spacing = 8;

        //public static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
    }
}
