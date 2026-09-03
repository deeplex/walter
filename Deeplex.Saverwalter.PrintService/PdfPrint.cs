// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace Deeplex.Saverwalter.PrintService
{

    public sealed class PdfPrint : IPrint<Document>
    {
        private static double leftMargin = 25;
        private static double rightMargin = 10;
        //private static double topMargin = 16.9;
        //private static double bottomMargin = 16.9;

        private static double pageWidth = 210;
        //private static double pageHeight = 297;

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
            para.Format.SpaceAfter = spacing;

            section.Add(para);
        }

        public void Text(string text)
        {
            var section = body.LastSection;
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = fontSize;
            paragraph.Format.Font.Name = font;
            paragraph.Format.SpaceAfter = spacing;

            paragraph.AddFormattedText(text);
        }
        public void PageBreak()
        {
            var section = body.AddSection();
            section.AddPageBreak();
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
            paragraph.Format.SpaceAfter = spacing;

            paragraph.AddFormattedText(text, TextFormat.Bold | TextFormat.Italic);
        }

        public void SubHeading(string text)
        {
            var section = body.LastSection;
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = fontSize;
            paragraph.Format.Font.Name = font;
            paragraph.Format.SpaceAfter = spacing;

            paragraph.AddFormattedText(text, TextFormat.Bold);
        }

        public void EntwurfHinweis(string grund)
        {
            var section = body.LastSection;
            var para = section.AddParagraph();
            para.Format.Font.Size = 14;
            para.Format.Font.Name = font;
            para.Format.SpaceAfter = spacing * 2;
            para.Format.Borders.Bottom = new MigraDoc.DocumentObjectModel.Border
            {
                Style = BorderStyle.Single,
                Width = 0.5
            };
            var fmt = para.AddFormattedText($"⚠ ENTWURF – {grund}", TextFormat.Bold);
            fmt.Font.Color = MigraDoc.DocumentObjectModel.Colors.Red;
        }

        public void EqHeizkostenV9_2(HkvoP9_2Berechnung b)
        {
            const double formulaSize = 14;
            var section = body.LastSection;

            Text("Berechnung Warmwasseranteil §9 Abs. 2 HKVO:");

            // Formel groß, zentriert und mathematisch gesetzt (kursive Variablen, echte
            // Subscripts) — Formelform aus §9 Abs. 2 HKVO: QW = 2,5 × V × (tw − 10°C) / Q.
            var formel = section.AddParagraph();
            formel.Format.Alignment = ParagraphAlignment.Center;
            formel.Format.SpaceBefore = spacing;
            formel.Format.SpaceAfter = spacing;

            FormattedText Sym(string text, bool sub = false)
            {
                var ft = formel.AddFormattedText(text);
                ft.Font.Name = font;
                ft.Size = formulaSize;
                ft.Italic = true;
                ft.Subscript = sub;
                return ft;
            }
            FormattedText Op(string text)
            {
                var ft = formel.AddFormattedText(text);
                ft.Font.Name = font;
                ft.Size = formulaSize;
                return ft;
            }

            Sym("Q"); Sym("W", sub: true); Op("  =  2,5 × "); Sym("V"); Op(" × (");
            Sym("t"); Sym("w", sub: true); Op(" − 10°C) / "); Sym("Q");

            // Legende: eingerückter, geschlossener Block statt loser Fließtext-Zeilen.
            var legende = section.AddParagraph();
            legende.Format.Font.Size = fontSize;
            legende.Format.Font.Name = font;
            legende.Format.LeftIndent = Unit.FromMillimeter(6);
            legende.Format.SpaceAfter = spacing;

            void LegendVar(string text, bool sub = false, bool bold = false)
            {
                var ft = legende.AddFormattedText(text);
                ft.Italic = true;
                ft.Subscript = sub;
                ft.Bold = bold;
            }
            void LegendRest(string text, bool bold = false)
            {
                var ft = legende.AddFormattedText(text);
                ft.Bold = bold;
            }

            LegendVar("V"); LegendRest($" = {Utils.Unit(b.V, "m³")}  (Warmwassermenge)");
            legende.AddLineBreak();
            LegendVar("Q"); LegendRest($" = {Utils.Unit(b.Q, "kWh")}  (Wärmemenge Allgemeinzähler)");
            legende.AddLineBreak();
            LegendVar("t"); LegendVar("w", sub: true);
            LegendRest($" = {Utils.Celsius((int)b.Tw)}  (geschätzte Warmwassertemperatur)");
            legende.AddLineBreak();
            LegendVar("Q", bold: true); LegendVar("W", sub: true, bold: true);
            LegendRest($" = {Utils.Prozent(b.Para9_2)}", bold: true);
        }

        private static string font = "Times New Roman";
        private static Unit fontSize = 11;
        private static Unit spacing = 8;

        //public static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
    }
}
