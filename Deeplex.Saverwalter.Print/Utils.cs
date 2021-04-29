using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deeplex.Saverwalter.Print
{
    public static class Utils
    {
        public static string Prozent(double d) => string.Format("{0:N2}%", d * 100);
        public static string Euro(double d) => string.Format("{0:N2}€", d);
        public static string Unit(double d, string unit) => string.Format("{0:N2} " + unit, d);
        public static string Celsius(double d) => string.Format("{0:N2}°C", d);
        public static string Celsius(int d) => d.ToString() + "°C";
        public static string Quadrat(double d) => string.Format("{0:N2} m²", d);
        public static string Datum(DateTime d) => d.ToString("dd.MM.yyyy");

        public static RunProperties Font()
        {
            var font = "Times New Roman";
            return new RunProperties(
                new RunFonts() { Ascii = font, HighAnsi = font, ComplexScript = font, },
                new FontSize() { Val = "22" }); // Size = 11
        }
        public static RunProperties Bold() => new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
        public static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
        public static Paragraph SubHeading(string str) => new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text(str)));
        public static Paragraph SubHeading(string str, bool _) => new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Break(), new Text(str)));
        public static Paragraph Heading(string str)
            => new Paragraph(Font(), new Run(Font(), new RunProperties(
                new Bold() { Val = OnOffValue.FromBoolean(true) },
                new Italic() { Val = OnOffValue.FromBoolean(true) }),
                new Text(str)));

        public static TableCell ContentCell(string str) => new TableCell(new Paragraph(Font(), NoSpace(), new Run(Font(), new Text(str))));
        public static TableCell ContentCell(string str, JustificationValues value)
            => new TableCell(
                new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(Font(), new Text(str))));

        public static TableCell ContentHead(string str) => new TableCell(new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text(str))));
        public static TableCell ContentHead(string pct, string str)
            => new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text(str))));
        public static TableCell ContentHead(string str, JustificationValues value)
            => new TableCell(
                new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(Font(), Bold(), new Text(str))));
        public static TableCell ContentHead(string pct, string str, JustificationValues value)
            => new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(Font(), Bold(), new Text(str))));

        public static TableCell ContentCellEnd(string str) => new TableCell(new Paragraph(Font(), new Run(Font(), new Text(str))));
    }
}
