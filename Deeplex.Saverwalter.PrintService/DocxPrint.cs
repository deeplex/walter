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

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using static Deeplex.Saverwalter.PrintService.Utils;

namespace Deeplex.Saverwalter.PrintService
{

    public sealed class DocxPrint : IPrint<Body>
    {
        public Body body { get; } = new(
            new SectionProperties(
            // Margins after DIN5008
            new PageMargin() { Left = 1418, Right = 567, Top = 958, Bottom = 958, },
            // DIN A4
            new PageSize() { Code = 9, Width = 11906, Height = 16838 }));

        public void Table(int[] widths, int[] justification, bool[] bold, bool[] underlined, string[][] cols)
        {
            TableCell ContentCell(string str, JustificationValues value, BorderValues bordervalue)
                => new TableCell(
                    new TableCellProperties(new BottomBorder() { Val = bordervalue, Size = 4 }),
                    new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                    new Run(Font(), new Text(str))));

            TableCell ContentCellWidth(string pct, string str, JustificationValues value, BorderValues bordervalue)
                => new TableCell(
                    new TableCellProperties(new BottomBorder() { Val = bordervalue, Size = 4 }),
                    new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                    new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                    new Run(Font(), new Text(str))));

            TableCell ContentHeadWidth(string pct, string str, JustificationValues value, BorderValues bordervalue)
                => new TableCell(
                    new TableCellProperties(new BottomBorder() { Val = bordervalue, Size = 4 }),
                    new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                    new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                    new Run(Font(), Bold(), new Text(str))));

            TableCell ContentHead(string str, JustificationValues value, BorderValues bordervalue)
                => new TableCell(
                    new TableCellProperties(new BottomBorder() { Val = bordervalue, Size = 4 }),
                    new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                    new Run(Font(), Bold(), new Text(str))));

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
                    headrow.Append(ContentHeadWidth((widths[i] * 50).ToString(), heads[i] ?? "", j[i], underlined[0] ? BorderValues.Single : BorderValues.None));
                }
                else
                {
                    headrow.Append(ContentCellWidth((widths[i] * 50).ToString(), heads[i] ?? "", j[i], underlined[0] ? BorderValues.Single : BorderValues.None));
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
                        cellrow.Append(ContentHead(row[k] ?? "", j[k], underlined[i] ? BorderValues.Single : BorderValues.None));
                    }
                    else
                    {
                        cellrow.Append(ContentCell(row[k] ?? "", j[k], underlined[i] ? BorderValues.Single : BorderValues.None));
                    }
                }
                table.Append(cellrow);
            }
            body.Append(table);
        }

        public void Paragraph(PrintRun[] runs)
        {
            var para = new Paragraph();
            for (var i = 0; i < runs.Length; ++i)
            {
                var run = runs[i];
                var r = new Run(Font(),
                    new RunProperties(
                        new Bold() { Val = OnOffValue.FromBoolean(run.Bold) },
                        new Underline() { Val = run.Underlined ? UnderlineValues.Single : UnderlineValues.None }),
                    new Text(run.Text) { Space = SpaceProcessingModeValues.Preserve });

                if (run.Tab)
                {
                    r.Append(new TabChar());
                }
                if (!run.NoBreak)
                {
                    if (i != runs.Length - 1)
                    {
                        r.Append(new Break());
                    }
                }
                para.Append(r);
            }

            body.Append(para);
        }

        public void Text(string s)
        {
            body.Append(new Paragraph(Font(), new Run(Font(), new Text(s))));
        }
        public void PageBreak()
        {
            body.Append(new Break() { Type = BreakValues.Page });
        }
        public void Break()
        {
            body.Append(new Paragraph(NoSpace()));
        }
        public void Heading(string str)
        {
            var para = new Paragraph(Font(), new Run(Font(), new RunProperties(
                new Bold() { Val = OnOffValue.FromBoolean(true) },
                new Italic() { Val = OnOffValue.FromBoolean(true) }),
                new Text(str)));

            body.Append(para);
        }
        public void SubHeading(string str)
        {
            var para = new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text(str)));
            body.Append(para);
        }

        public void EntwurfHinweis(string grund)
        {
            var rp = new RunProperties(
                new RunFonts() { Ascii = font, HighAnsi = font, ComplexScript = font },
                new FontSize() { Val = "28" },
                new Bold(),
                new Color() { Val = "FF0000" });
            var para = new Paragraph(
                Font(),
                new ParagraphProperties(
                    new SpacingBetweenLines() { After = "240" },
                    new ParagraphBorders(new BottomBorder() { Val = BorderValues.Single, Size = 6, Space = 1 })),
                new Run(rp, new Text($"⚠ ENTWURF – {grund}")));
            body.Append(para);
        }

        private static string font = "Times New Roman";
        private static int fontSize = 11;

        private static RunProperties Font() => new RunProperties(
                new RunFonts() { Ascii = font, HighAnsi = font, ComplexScript = font, },
                new FontSize() { Val = (fontSize * 2).ToString() }); // Don't know why * 2
        private static RunProperties Bold() => new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
        private static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });

        public void EqHeizkostenV9_2(HkvoP9_2Berechnung b)
        {
            DocumentFormat.OpenXml.Math.RunProperties MathRp() =>
                new(new RunFonts() { Ascii = "Cambria Math", HighAnsi = "Cambria Math" });
            DocumentFormat.OpenXml.Math.Run T(string str) =>
                new(MathRp(), new DocumentFormat.OpenXml.Math.Text(str));
            DocumentFormat.OpenXml.Math.Subscript Tw() =>
                new(new DocumentFormat.OpenXml.Math.Base(T("t")),
                    new DocumentFormat.OpenXml.Math.SubArgument(T("w")));
            DocumentFormat.OpenXml.Math.Fraction Frac(string num, string den) =>
                new(new DocumentFormat.OpenXml.Math.Numerator(T(num)),
                    new DocumentFormat.OpenXml.Math.Denominator(T(den)));
            DocumentFormat.OpenXml.Math.Fraction Units() => Frac("kWh", "m³ × K");

            body.Append(new Paragraph(Font(), new Run(Font(), new Text("Berechnung Warmwasseranteil §9 Abs. 2 HKVO:"))));
            body.Append(new Paragraph(Font(), NoSpace(),
                new DocumentFormat.OpenXml.Math.OfficeMath(
                    T("2,5 ×"), Frac("V", "Q"), T(" × ("), Tw(), T("-10°C)"), Units(), T(" ⟹ "),
                    T("2,5 ×"), Frac(Unit(b.V, "m³"), Unit(b.Q, "kWh")), T(" × ("),
                    T(Celsius((int)b.Tw)), T("-10°C)"), Units(), T(" = "), T(Prozent(b.Para9_2)))));
            body.Append(new Paragraph(Font(), new Run(Font(), new Text($"V = {Unit(b.V, "m³")} = Warmwassermenge"))));
            body.Append(new Paragraph(Font(), new Run(Font(), new Text($"Q = {Unit(b.Q, "kWh")} = Wärmemenge Allgemeinzähler"))));
            body.Append(new Paragraph(Font(), new Run(Font(), new Text($"tw = {Celsius((int)b.Tw)} = geschätzte Temperatur Warmwasser"))));
        }
    }
}
