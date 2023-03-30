using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Immutable;
using PdfSharpCore.Pdf;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using System.IO;

namespace Deeplex.Saverwalter.PrintService
{
    public static class PdfIntegration
    {
        public static void SaveAsPdf(this IBetriebskostenabrechnung b, Stream stream)
        {
            var document = TPrint<PdfDocument>.Print(b, new PdfPrint());
            // Second argument states that the stream shouldn't be closed yet.
            document.Save(stream, false);
        }
    }

}
