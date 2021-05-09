using Deeplex.Saverwalter.Model;
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

        public static bool SaveAsDocx(this Betriebskostenabrechnung b, string filepath)
        {
            var body = new Body(
                new SectionProperties(
                // Margins after DIN5008
                new PageMargin() { Left = 1418, Right = 567, Top = 958, Bottom = 958, },
                // DIN A4
                new PageSize() { Code = 9, Width = 11906, Height = 16838 }));

            FirstPage.FirstPage.Fill(body, b);
            SecondPage.SecondPage.Fill(body, b);
            ThirdPage.ThirdPage.Fill(body, b);

            bool MakeSpace(string path)
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
            var ok = MakeSpace(filepath);
            if (ok)
            {
                CreateWordDocument(filepath, body);
            }
            return ok;
        }
    }
}
