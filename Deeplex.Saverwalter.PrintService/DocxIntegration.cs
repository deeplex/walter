using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Deeplex.Saverwalter.PrintService
{
    public static class DocxIntegration
    {
        private static void CreateWordDocument(Stream stream, Body body)
        {
            using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
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
                wordDocument.MainDocumentPart?.Document.AppendChild(body);
            }
        }

        public static void SaveAsDocx(this Betriebskostenabrechnung abrechnung, Stream stream)
        {
            var body = TPrint<Body>.Print(abrechnung, new DocxPrint());

            CreateWordDocument(stream, body);
        }
    }

}
