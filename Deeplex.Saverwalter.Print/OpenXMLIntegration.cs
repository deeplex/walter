﻿using Deeplex.Saverwalter.Model;
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
                File.Delete(filepath);
                throw;
            }
            wordDocument.MainDocumentPart.Document.AppendChild(body);
        }

        public static Body DinA4()
            => new Body(
                new SectionProperties(
                // Margins after DIN5008
                new PageMargin() { Left = 1418, Right = 567, Top = 958, Bottom = 958, },
                // DIN A4
                new PageSize() { Code = 9, Width = 11906, Height = 16838 }));

        public static void SaveAsDocx(this ImmutableList<ErhaltungsaufwendungWohnung> l, string filepath)
        {
            var body = DinA4();
            foreach (var e in l)
            {
                if (!e.Liste.IsEmpty)
                {
                    TPrint<Body>.Print(e, new WordPrint());
                }
            }

            CreateWordDocument(filepath, body);
        }

        public static void SaveAsDocx(this IErhaltungsaufwendungWohnung e, string filepath)
        {
            var body = DinA4();
            if (!e.Liste.IsEmpty)
            {
                TPrint<Body>.Print(e, new WordPrint());
            }

            CreateWordDocument(filepath, body);
        }

        public static void SaveAsDocx(this IBetriebskostenabrechnung b, string filepath)
        {
            var body = TPrint<Body>.Print(b, new WordPrint());

            CreateWordDocument(filepath, body);
        }
    }

}