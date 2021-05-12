using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.ErhaltungsaufwendungListe;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Deeplex.Saverwalter.Model.Betriebskostenabrechnung;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print
{
    public static class OpenXMLIntegration
    {
        private static void CreateWordDocument(string filepath, Body body)
        {
            using var wordDocument = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document);
            try
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
            }
            catch (Exception)
            {
                wordDocument.Dispose();
                throw;
            }
            wordDocument.MainDocumentPart.Document.AppendChild(body);
        }

        private static Body DinA4()
            => new Body(
                new SectionProperties(
                // Margins after DIN5008
                new PageMargin() { Left = 1418, Right = 567, Top = 958, Bottom = 958, },
                // DIN A4
                new PageSize() { Code = 9, Width = 11906, Height = 16838 }));

        public static bool SaveAsDocx(this ErhaltungsaufwendungListe e, string filepath)
        {
            var body = DinA4();

            body.Append(new Paragraph(
                Heading(Anschrift(e.Wohnung.Adresse) + ", " + e.Wohnung.Bezeichnung)));

            var table = new Table(
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

            var ok = MakeSpace(filepath);
            if (ok)
            {
                CreateWordDocument(filepath, body);
            }
            return ok;
        }

        public static bool SaveAsDocx(this Betriebskostenabrechnung b, string filepath)
        {
            var body = DinA4();

            FirstPage.FirstPage.Fill(body, b);
            SecondPage.SecondPage.Fill(body, b);
            ThirdPage.ThirdPage.Fill(body, b);
            
            var ok = MakeSpace(filepath);
            if (ok)
            {
                CreateWordDocument(filepath, body);
            }
            return ok;
        }

        public static bool MakeSpace(string path)
        {
            var ok = true;
            if (File.Exists(path))
            {
                var dirname = Path.GetDirectoryName(path);
                var filename = Path.GetFileNameWithoutExtension(path);
                var extension = Path.GetExtension(path);
                var newPath = Path.Combine(dirname, filename + ".old" + extension);
                ok = MakeSpace(newPath);
                try
                {
                    File.Move(path, newPath);
                }
                catch
                {
                    return false;
                }
            }
            return ok;
        }
    }
}
