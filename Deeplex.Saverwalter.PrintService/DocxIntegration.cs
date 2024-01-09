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
