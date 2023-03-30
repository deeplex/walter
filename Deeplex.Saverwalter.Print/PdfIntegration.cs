using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Text;

namespace Deeplex.Saverwalter.PrintService
{
    public static class PdfIntegration
    {
        public static void SaveAsPdf(this IBetriebskostenabrechnung b, Stream stream)
        {
            var document = TPrint<Document>.Print(b, new PdfPrint());

            var renderer = new PdfDocumentRenderer(true);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            renderer.Document = document;
            renderer.DocumentRenderer = new DocumentRenderer(document);
            renderer.RenderDocument();
            // Second argument states that the stream shouldn't be closed yet.
            renderer.PdfDocument.Save(stream, false);
        }
    }

}
