using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.ErhaltungsaufwendungListe;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Immutable;
using System.IO;

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

        public static bool SaveAsDocx(this ImmutableList<ErhaltungsaufwendungWohnung> l, string filepath)
        {
            var body = DinA4();
            foreach (var e in l)
            {
                if (!e.Liste.IsEmpty)
                {
                    Erhaltungsaufwendung.ErhaltungsaufwendungWohnungBody(body, e);
                }
            }

            return MakeSpaceAndCreateWordDocument(filepath, body);
        }

        public static bool SaveAsDocx(this ErhaltungsaufwendungWohnung e, string filepath)
        {
            var body = DinA4();
            Erhaltungsaufwendung.ErhaltungsaufwendungWohnungBody(body, e);

            if (e.Liste.IsEmpty)
            {
                return false;
            }

            return MakeSpaceAndCreateWordDocument(filepath, body);
        }

        public static bool SaveAsDocx(this Betriebskostenabrechnung b, string filepath)
        {
            var body = DinA4();
            FirstPage.FirstPage.Fill(body, b);
            SecondPage.SecondPage.Fill(body, b);
            ThirdPage.ThirdPage.Fill(body, b);

            return MakeSpaceAndCreateWordDocument(filepath, body);
        }

        private static bool MakeSpaceAndCreateWordDocument(string filepath, Body body)
        {
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
