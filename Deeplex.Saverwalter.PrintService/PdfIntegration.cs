using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Text;

namespace Deeplex.Saverwalter.PrintService
{
    public static class PdfIntegration
    {
        public static void SaveAsPdf(this Betriebskostenabrechnung abrechnung, Stream stream)
        {
            var document = TPrint<Document>.Print(abrechnung, new PdfPrint());

            var renderer = new PdfDocumentRenderer();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            renderer.Document = document;
            renderer.DocumentRenderer = new DocumentRenderer(document);
            renderer.RenderDocument();
            // Second argument states that the stream shouldn't be closed yet.
            renderer.PdfDocument.Save(stream, false);
        }
    }

}
