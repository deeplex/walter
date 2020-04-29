using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.Word;

namespace Deeplex.Saverwalter.Print
{
    public static class Betriebskostenabrechnung
    {
        /*
         * Die Betriebskostenabrechnung kann hier auf einen Schlag,
         * am besten für eine Adresse gemacht werden.
         * Hierfür wird am besten die Adresse und ein Jahr übergeben.
         * Dann werden alle Verträge die zu dieser Adresse gehören aus
         * einem bestimmten Jahr zusammengetragen und es sollte über
         * diese iteriert werden können.
         * 
         * Jeder Vertrag in der Liste bekommt entsprechend eine eigene
         * Betriebskostenabrechnung.
         * 
         * TODO Funktion überladen um verschiedene Scopes zu erlauben
         * Abrechnung für Mieter, Wohnung, Alle (?)
         */
        public static void Create()
        {
            using var db = new SaverwalterContext();

            var filepath = "walter.docx"; // TODO: Benennung klüger machen.
            using var wordDocument = CreateNew(filepath);
            /* 
             * TODO: Als erstes wird allerdings (im Gegensatz zum Kommentar von "erstellen")
             * nur ein Vertrag abgehandelt. Das iterieren über eine Liste wird an dieser
             * Stelle also noch nicht implementiert.
            */
            wordDocument.MainDocumentPart.Document.AppendChild(db.PrintBetriebskostenabrechnung(1));
        }
        public static WordprocessingDocument CreateNew(string filepath)
        {
            var wordDocument = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document);

            try
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                return wordDocument;
            }
            catch (Exception)
            {
                wordDocument.Dispose();
                throw;
            }
        }
        public static Body PrintBetriebskostenabrechnung(this SaverwalterContext db, int rowid)
        {
            var vertrag = db.Vertraege
                .Where(v => v.rowid == rowid)
                .Include(v => v.Vermieter)
                .Include(v => v.Ansprechpartner)
                    .ThenInclude(k => k.Adresse)
                .Include(v => v.Mieter)
                    .ThenInclude(m => m.Kontakt)
                .Include(v => v.Wohnungen)
                    .ThenInclude(m => m.Wohnung)
                        .ThenInclude(w => w.Adresse)
                .First();

            var body = new Body(
                AnschriftVermieter(vertrag.Vermieter, vertrag.Ansprechpartner),
                PostalischerVermerk(vertrag.Mieter),
                PrintDate(),
                Betreff(vertrag));

            return body;
        }

        private static Paragraph AnschriftVermieter(JuristischePerson Vermieter, Kontakt Ansprechpartner)
        {
            var para = new Paragraph(new ParagraphProperties(new Tabs(new TabStop()
            {
                Val = TabStopValues.Right,
                Position = 8647, // TODO adapt to right margin
            })));

            var run = para.AppendChild(new Run(
                new RunProperties(new Bold()
                {
                    Val = OnOffValue.FromBoolean(true),
                }),
                new Text(Vermieter.Bezeichnung),
                new Break(), // 1
                new Text(Ansprechpartner.Vorname + " " + Ansprechpartner.Nachname),
                new TabChar(),
                new Text("Tel.: " + Ansprechpartner.Telefon),
                new Break(), // 2
                new Text(Ansprechpartner.Adresse!.Strasse + " " + Ansprechpartner.Adresse.Hausnummer),
                new TabChar(),
                new Text("Fax: " + Ansprechpartner.Fax),
                new Break(), // 3
                new Text(Ansprechpartner.Adresse.Postleitzahl + " " + Ansprechpartner.Adresse.Stadt),
                new TabChar(),
                new Text("E-Mail: " + Ansprechpartner.Email)));

            run.Append(Enumerable.Range(0, 9 - 3).Select(_ => new Break()));

            return para;
        }

        private static Paragraph PostalischerVermerk(IList<Mieter> Mieter)
        {
            var para = new Paragraph();
            var run = para.AppendChild(new Run());

            int counter = 6;
            foreach (var m in Mieter)
            {
                var anrede = m.Kontakt.Anrede == Anrede.Herr ? "Herrn " : m.Kontakt.Anrede == Anrede.Frau ? "Frau " : "";
                run.Append(new Text(anrede + m.Kontakt.Vorname + " " + m.Kontakt.Nachname));
                run.Append(new Break());
                counter--;
            }

            run.Append(Enumerable.Range(0, counter).Select(_ => new Break()));
            
            return para;
        }

        private static Paragraph PrintDate()
        {
            var para = new Paragraph(new ParagraphProperties(new Justification
            {
                Val = JustificationValues.Right,
            }),
            new Run(
                new Text(DateTime.Today.ToShortDateString())));

            return para;
        }

        private static Paragraph Betreff(Vertrag Vertrag)
        {
            var Mieter = Vertrag.Mieter;
            var Wohnungen = Vertrag.Wohnungen;

            var Jahr = 2018;
            var Abrechnungsbeginn = new DateTime(Jahr, 1, 1);
            var Abrechnungsende = new DateTime(Jahr, 12, 31);

            var Nutzungsbeginn = Abrechnungsbeginn > Vertrag.Beginn
                ? Abrechnungsbeginn : Vertrag.Beginn;
            var Nutzungsende = Vertrag.Ende is DateTime && Abrechnungsende > Vertrag.Ende
                ? (DateTime)Vertrag.Ende : Abrechnungsende;

           
            var Mieterliste = string.Join(", ",
                Mieter.Select(m => m.Kontakt.Vorname + " " + m.Kontakt.Nachname));
            var Wohnungsliste = string.Join(", ",
                Wohnungen.Select(w => w.Wohnung.Adresse.Strasse + " " + w.Wohnung.Adresse.Hausnummer));

            var para = new Paragraph(
                new Run(
                    new RunProperties(new Bold()
                    {
                        Val = OnOffValue.FromBoolean(true)
                    }),
                    new Text("Betriebskostenabrechnung 2018"), // TODO 2018 is hard coded.
                    new Break()),
                new Run(
                    new Text("Mieter: " + Mieterliste),
                    new Break(),
                    new Text("Mietobjekt" + (Wohnungen.Count > 1 ? "e: " : ": " + Wohnungsliste)),
                    new Break(),
                    new Text("Abrechnungszeitraum: "),
                    new TabChar(),
                    new Text(Abrechnungsbeginn.ToShortDateString() + " - " + Abrechnungsende.ToShortDateString()),
                    new Break(),
                    new Text("Nutzungszeitraum: "),
                    new TabChar(),
                    new Text(Nutzungsbeginn.ToShortDateString() + " - " + Nutzungsende.ToShortDateString())));     

            return para;
        }
    }
}
