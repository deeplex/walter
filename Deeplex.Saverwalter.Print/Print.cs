using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
                .Include(v => v.Wohnung!)
                    .ThenInclude(w => w.Adresse)
                        .ThenInclude(a => a.KalteBetriebskosten)
                .First();

            var result = 123.45; // TODO has to be calculated.

            var body = new Body(
                // p.1
                AnschriftVermieter(vertrag.Vermieter, vertrag.Ansprechpartner),
                PostalischerVermerk(vertrag.Mieter),
                PrintDate(),
                Betreff(vertrag),
                Ergebnis(vertrag.Mieter, result),
                GenericText(),
                new Break() { Type = BreakValues.Page },
                // p.2
                Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)"),
                ExplainUmlageschluessel(),
                Heading("Erläuterungen zu einzelnen Betriebskostenarten"),
                ExplainKalteBetriebskosten(vertrag.Wohnung!.Adresse.KalteBetriebskosten.Where(k => k.Beschreibung is string).ToList()),
                new Break() { Type = BreakValues.Page },
                Heading("Abrechnung der Nebenkosten (kalten Betriebskosten)"),
                MietHeader(vertrag.Mieter, vertrag.Wohnung)
            );

            return body;
        }

        static Paragraph Heading(string str)
        {
            return new Paragraph(new Run(new RunProperties(
                new Bold() { Val = OnOffValue.FromBoolean(true) },
                new Italic() { Val = OnOffValue.FromBoolean(true) }),
                new Text(str)));
        }

        static Paragraph MietHeader(IList<Mieter> Mieter, Wohnung Wohnung)
        {
            var Header = string.Join(", ", Mieter.Select(m => (
                m.Kontakt.Anrede == Anrede.Herr ? "Herrn " :
                m.Kontakt.Anrede == Anrede.Frau ? "Frau " :
                m.Kontakt.Vorname) + " " + m.Kontakt.Nachname)) +
                " (" + Wohnung.Adresse.Strasse + " " + Wohnung.Adresse.Hausnummer + ", " + Wohnung.Bezeichnung + ")";

            return new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(new Text(Header)));
        }

        private static Paragraph AnschriftVermieter(JuristischePerson Vermieter, Kontakt Ansprechpartner)
        {
            // TODO Make this a table.

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
            var Adresse = Vertrag.Wohnung!.Adresse;

            var Jahr = 2018;
            var Abrechnungsbeginn = new DateTime(Jahr, 1, 1);
            var Abrechnungsende = new DateTime(Jahr, 12, 31);

            var Nutzungsbeginn = Abrechnungsbeginn > Vertrag.Beginn
                ? Abrechnungsbeginn : Vertrag.Beginn;
            var Nutzungsende = Vertrag.Ende is DateTime && Abrechnungsende > Vertrag.Ende
                ? (DateTime)Vertrag.Ende : Abrechnungsende;


            var Mieterliste = string.Join(", ",
                Mieter.Select(m => m.Kontakt.Vorname + " " + m.Kontakt.Nachname));

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
                    new Text("Mietobjekt: " + Adresse.Strasse + " " + Adresse.Hausnummer + ", " + Vertrag.Wohnung.Bezeichnung),
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

        private static Paragraph Ergebnis(IList<Mieter> Mieter, double result)
        {
            var gruss = Mieter.Aggregate("", (r, m) => r + (
                m.Kontakt.Anrede == Anrede.Herr ? "sehr geehrter Herr " :
                m.Kontakt.Anrede == Anrede.Frau ? "sehr geehrte Frau " :
                m.Kontakt.Vorname) + m.Kontakt.Nachname + ", ");
            var Gruss = gruss.Remove(1).ToUpper() + gruss.Substring(1);

            var resultTxt1 = "Die Abrechnung schließt mit " + (result > 0 ?
                "einer Nachforderung" : "einem Guthaben") + " in Höhe von: ";

            var refund = new Run(
                new Text("Dieser Betrag wird über die von Ihnen angegebene Bankverbindung erstattet."));

            var demand = new Run(
                new Text("Bitte überweisen Sie diesen Betrag auf das Ihnen bekannte Mietkonto bei der "),
                new Break(),
                new Text("Bankkonto IBAN TODO!")); // TODO Konto fehlt noch...

            var para = new Paragraph(
                new Run(
                    new Text(Gruss),
                    new Break(),
                    new Text("wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. " +
                        resultTxt1),
                    new TabChar()),
                new Run(
                    new RunProperties(
                        new Bold()
                        {
                            Val = OnOffValue.FromBoolean(true),
                        },
                    new Underline()
                    {
                        Val = UnderlineValues.Single,
                    },
                    new Text(string.Format("{0:N2}€", result)),
                    new Break()
                    )),
                result > 0 ? refund : demand);
            return para;
        }

        private static Paragraph GenericText()
        {
            // TODO Text auf Anwesenheit von Heizung oder so testen und anpassen.

            return new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Both, }),
                new Run(new Text("Die Abrechnung betrifft zunächst die mietvertraglich vereinbarten Nebenkosten (die kalten Betriebskosten). Die Kosten für die Heizung und für die Erwärmung von Wasser über die Heizanlage Ihres Wohnhauses(warme Betriebskosten) werden gesondert berechnet, nach Verbrauch und Wohn -/ Nutzfläche auf die einzelnen Wohnungen umgelegt(= „Ihre Heizungsrechnung“) und mit dem Ergebnis aus der Aufrechnung Ihrer Nebenkosten und der Summe der von Ihnen geleisteten Vorauszahlungen verrechnet. Bei bestehenden Mietrückständen ist das Ergebnis der Abrechnung zusätzlich mit den Mietrückständen verrechnet. Gegebenenfalls bestehende Mietminderungen / Ratenzahlungsvereinbarungen sind hier nicht berücksichtigt, haben aber weiterhin für den vereinbarten Zeitraum Bestand. Aufgelöste oder gekündigte Mietverhältnisse werden durch dieses Schreiben nicht neu begründet. Die Aufstellung, Verteilung und Erläuterung der Gesamtkosten, die Berechnung der Kostenanteile, die Verrechnung der geleisteten Vorauszahlungen und gegebenenfalls die Neuberechnung der monatlichen Vorauszahlungen entnehmen Sie bitte den folgenden Seiten.")));
        }


        private static Table ExplainUmlageschluessel()
        {
            static RunProperties Bold() { return new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }); }
            static ParagraphProperties NoSpace() { return new ParagraphProperties(new SpacingBetweenLines() { After = "0" }); }
            static Paragraph Content(string str) { return new Paragraph(NoSpace(), new Run(new Text(str))); }

            return new Table(
                // This cell defines the width of the left cell.
                new TableRow(
                    new TableCell(
                        new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "1701" }),
                        new Paragraph(NoSpace(), new Run(Bold(), new Text("Umlageschlüssel")))),
                    new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text("Bedeutung"))))),
                new TableRow(
                    new TableCell(Content("n. WF.")),
                    new TableCell(Content("nach Wohn-/Nutzfläche in m²"))),
                new TableRow(
                    new TableCell(Content("n. NE.")),
                    new TableCell(Content("nach Anzahl der Wohn-/Nutzeinheiten"))),
                new TableRow(
                    new TableCell(Content("n. Pers.")),
                    new TableCell(Content("nach Personenzahl/Anzahl der Bewohner"))),
                new TableRow( // This row has SpacingBeetweenLine.
                    new TableCell(new Paragraph(new Run(new Text("n. Verb.")))),
                    new TableCell(new Paragraph(new Run(new Text("nach Verbrauch (in m³ oder in kWh)"))))),

                new TableRow(
                    new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text("Umlageweg")))),
                    new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text("Beschreibung"))))),
                new TableRow(
                    new TableCell(Content("n. WF.")),
                    new TableCell(Content("Kostenanteil = Kosten je Quadratmeter Wohn-/Nutzfläche mal Anteil Fläche je Wohnung."))),
                new TableRow(
                    new TableCell(Content("n. NE.")),
                    new TableCell(Content("Kostenanteil = Kosten je Wohn-/Nutzeinheit."))),
                new TableRow(
                    new TableCell(Content("n. Pers.")),
                    new TableCell(Content("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung."))),
                new TableRow( // This row has SpacingBeetweenLine.
                    new TableCell(new Paragraph(new Run(new Text("n. Verb.")))),
                    new TableCell(new Paragraph(new Run(new Text("Kostenanteil = Kosten je Verbrauchseinheit mal individuelle Verbrauchsmenge in Kubikmetern oder Kilowattstunden."))))),

                new TableRow(
                    new TableCell(new Paragraph(new Run(new Text("Anmerkung: ")))),
                    new TableCell(new Paragraph(new Run(new Text("Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger)Kostenanteil"))))));
        }

        private static Paragraph ExplainKalteBetriebskosten(IList<KalteBetriebskostenpunkt> items)
        {
            var para = new Paragraph();

            foreach (var item in items)
            {
                para.Append(
                    new Run(
                        new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                        new Text(item.Bezeichnung.ToDescriptionString() + ": ") { Space = SpaceProcessingModeValues.Preserve }),
                    new Run(
                        new Text(item.Beschreibung),
                        new Break()));
            }

            return para;
        }
    }
}
