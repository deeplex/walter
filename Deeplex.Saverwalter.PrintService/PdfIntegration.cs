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

using System.Text;
using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace Deeplex.Saverwalter.PrintService
{
    public class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            return File.ReadAllBytes(fontPath);
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Times New Roman", StringComparison.CurrentCultureIgnoreCase))
            {
                return new FontResolverInfo("Times New Roman");
            }
            else if (familyName.Equals("Arial", StringComparison.CurrentCultureIgnoreCase))
            {
                return new FontResolverInfo("Arial");
            }
            else
            {
                return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
            }
        }
    }

    public static class PdfIntegration
    {
        public static void SaveAsPdf(this Betriebskostenabrechnung abrechnung, Stream stream)
        {
            GlobalFontSettings.FontResolver = new CustomFontResolver();
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
