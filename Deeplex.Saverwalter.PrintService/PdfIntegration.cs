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
    // Liberation Serif 2.1.5 is bundled under the SIL Open Font License 1.1.
    public class CustomFontResolver : IFontResolver
    {
        // Serif variants (metrically equivalent to Times New Roman)
        private const string SerifRegular = "LiberationSerif-Regular";
        private const string SerifBold = "LiberationSerif-Bold";
        private const string SerifItalic = "LiberationSerif-Italic";
        private const string SerifBoldItalic = "LiberationSerif-BoldItalic";

        // Sans variants (metrically equivalent to Arial, required by MigraDoc)
        private const string SansRegular = "LiberationSans-Regular";
        private const string SansBold = "LiberationSans-Bold";
        private const string SansItalic = "LiberationSans-Italic";
        private const string SansBoldItalic = "LiberationSans-BoldItalic";

        // Mono variants (metrically equivalent to Courier New, required by MigraDoc)
        private const string MonoRegular = "LiberationMono-Regular";
        private const string MonoBold = "LiberationMono-Bold";
        private const string MonoItalic = "LiberationMono-Italic";
        private const string MonoBoldItalic = "LiberationMono-BoldItalic";

        private static readonly string ResourceNamespace =
            typeof(CustomFontResolver).Namespace!;

        public byte[] GetFont(string faceName)
        {
            var resourceName = faceName switch
            {
                SerifRegular    => $"{ResourceNamespace}.Fonts.LiberationSerif-Regular.ttf",
                SerifBold       => $"{ResourceNamespace}.Fonts.LiberationSerif-Bold.ttf",
                SerifItalic     => $"{ResourceNamespace}.Fonts.LiberationSerif-Italic.ttf",
                SerifBoldItalic => $"{ResourceNamespace}.Fonts.LiberationSerif-BoldItalic.ttf",
                MonoRegular     => $"{ResourceNamespace}.Fonts.LiberationMono-Regular.ttf",
                MonoBold        => $"{ResourceNamespace}.Fonts.LiberationMono-Bold.ttf",
                MonoItalic      => $"{ResourceNamespace}.Fonts.LiberationMono-Italic.ttf",
                MonoBoldItalic  => $"{ResourceNamespace}.Fonts.LiberationMono-BoldItalic.ttf",
                SansRegular     => $"{ResourceNamespace}.Fonts.LiberationSans-Regular.ttf",
                SansBold        => $"{ResourceNamespace}.Fonts.LiberationSans-Bold.ttf",
                SansItalic      => $"{ResourceNamespace}.Fonts.LiberationSans-Italic.ttf",
                SansBoldItalic  => $"{ResourceNamespace}.Fonts.LiberationSans-BoldItalic.ttf",
                _ => $"{ResourceNamespace}.Fonts.LiberationSerif-Regular.ttf"
            };

            var assembly = typeof(CustomFontResolver).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Embedded font resource '{resourceName}' not found. " +
                    $"Available: {string.Join(", ", assembly.GetManifestResourceNames())}");

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Map Times New Roman (and the bundled Liberation Serif name) to the
            // embedded Liberation Serif variants, which are metrically equivalent.
            if (familyName.Equals("Times New Roman", StringComparison.OrdinalIgnoreCase) ||
                familyName.StartsWith("Liberation Serif", StringComparison.OrdinalIgnoreCase))
            {
                var face = (isBold, isItalic) switch
                {
                    (true, true)   => SerifBoldItalic,
                    (true, false)  => SerifBold,
                    (false, true)  => SerifItalic,
                    _              => SerifRegular
                };
                return new FontResolverInfo(face);
            }

            // MigraDoc requires Courier New for its internal error font.
            if (familyName.Equals("Courier New", StringComparison.OrdinalIgnoreCase) ||
                familyName.StartsWith("Liberation Mono", StringComparison.OrdinalIgnoreCase))
            {
                var face = (isBold, isItalic) switch
                {
                    (true, true)   => MonoBoldItalic,
                    (true, false)  => MonoBold,
                    (false, true)  => MonoItalic,
                    _              => MonoRegular
                };
                return new FontResolverInfo(face);
            }

            // MigraDoc requires Arial for its internal fonts.
            if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase) ||
                familyName.StartsWith("Liberation Sans", StringComparison.OrdinalIgnoreCase))
            {
                var face = (isBold, isItalic) switch
                {
                    (true, true)   => SansBoldItalic,
                    (true, false)  => SansBold,
                    (false, true)  => SansItalic,
                    _              => SansRegular
                };
                return new FontResolverInfo(face);
            }

            // Fall back to Liberation Serif for any unknown font family
            // (avoids PlatformFontResolver failures on Linux for uninstalled fonts)
            var fallbackFace = (isBold, isItalic) switch
            {
                (true, true)   => SerifBoldItalic,
                (true, false)  => SerifBold,
                (false, true)  => SerifItalic,
                _              => SerifRegular
            };
            return new FontResolverInfo(fallbackFace);
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
