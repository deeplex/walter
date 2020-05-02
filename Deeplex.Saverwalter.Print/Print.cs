using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

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
            wordDocument.MainDocumentPart.Document.AppendChild(db.PrintBetriebskostenabrechnung(6));
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
                            .ThenInclude(k => k.Rechnungen)
                .Include(v => v.Wohnung!)
                    .ThenInclude(w => w.Adresse)
                        .ThenInclude(a => a.Wohnungen)
                            .ThenInclude(w2 => w2.Vertraege)
                .Include(v => v.Wohnung!)
                .First();


            var result = 123.45; // TODO has to be calculated.
            var Jahr = 2018; // TODO Shouldn't be hard coded and btw must not be 1.1 to 31.12...
            var Abrechnungsbeginn = new DateTime(Jahr, 1, 1);
            var Abrechnungsende = new DateTime(Jahr, 12, 31);

            var body = new Body(new SectionProperties(
                // Margins after DIN5008
                new PageMargin() { Left = 1418, Right = 567, Top = 958, Bottom = 958, },
                // DIN A4
                new PageSize() { Code = 1 }),
                // p.1
                AnschriftVermieter(vertrag.Vermieter, vertrag.Ansprechpartner),
                PostalischerVermerk(vertrag.Mieter),
                PrintDate(),
                Betreff(vertrag, Abrechnungsbeginn, Abrechnungsende),
                Ergebnis(vertrag.Mieter, result),
                GenericText(),
                new Break() { Type = BreakValues.Page },
                // p.2
                Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)"),
                ExplainUmlageschluessel(),
                Heading("Erläuterungen zu einzelnen Betriebskostenarten"),
                ExplainKalteBetriebskosten(vertrag.Wohnung!.Adresse.KalteBetriebskosten.Where(k => k.Beschreibung is string).ToList()),
                new Break() { Type = BreakValues.Page },
                // p.3
                Heading("Abrechnung der Nebenkosten (kalten Betriebskosten)"),
                MietHeader(vertrag.Mieter, vertrag.Wohnung),
                SubHeading("Angaben zur Abrechnungseinheit"),
                Abrechnungseinheit(vertrag.Wohnung.Adresse, Abrechnungsbeginn, Abrechnungsende),
                Abrechnungswohnung(vertrag, vertrag.Wohnung, Abrechnungsbeginn, Abrechnungsende),
                new Paragraph(), // Necessary to split there tables
                ErmittlungEinheiten(vertrag, vertrag.Wohnung, Abrechnungsbeginn, Abrechnungsende),
                SubHeading("Ermittlung der kalten Betriebskosten"),
                ErmittlungKosten(vertrag, vertrag.Wohnung, Abrechnungsbeginn, Abrechnungsende, Jahr)
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

        static Paragraph SubHeading(string str)
        {
            return new Paragraph(new Run(
                new RunProperties(
                    new Bold() { Val = OnOffValue.FromBoolean(true) },
                    new SpacingBetweenLines() { After = "0" }),
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

        private static Table AnschriftVermieter(JuristischePerson Vermieter, Kontakt Ansprechpartner)
        {
            static TableCell ContentCellRight(string str) => new TableCell(
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(new Text(str))));

            var table = new Table(
                new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                new TableRow(
                    ContentCell(Vermieter.Bezeichnung),
                    ContentCellRight("")),
                new TableRow(
                    ContentCell(Ansprechpartner.Vorname + " " + Ansprechpartner.Nachname),
                    ContentCellRight("Tel.: " + Ansprechpartner.Telefon)),
                new TableRow(
                    ContentCell(Ansprechpartner.Adresse!.Strasse + " " + Ansprechpartner.Adresse.Hausnummer),
                    ContentCellRight("Fax: " + Ansprechpartner.Fax)),
                new TableRow(
                    ContentCell(Ansprechpartner.Adresse.Postleitzahl + " " + Ansprechpartner.Adresse.Stadt),
                    ContentCellRight("E-Mail: " + Ansprechpartner.Email)));
            table.Append(Enumerable.Range(0, 9 - 4).Select(_ => new Break()));

            return table;
        }

        private static Paragraph PostalischerVermerk(IList<Mieter> Mieter)
        {
            var run = new Run();
            int counter = 6;

            foreach (var m in Mieter)
            {
                var anrede = m.Kontakt.Anrede == Anrede.Herr ? "Herrn " : m.Kontakt.Anrede == Anrede.Frau ? "Frau " : "";
                run.Append(new Text(anrede + m.Kontakt.Vorname + " " + m.Kontakt.Nachname));
                run.Append(new Break());
                counter--;
            }

            run.Append(Enumerable.Range(0, counter).Select(_ => new Break()));

            return new Paragraph(run);
        }

        private static Paragraph PrintDate()
        {
            return new Paragraph(new ParagraphProperties(new Justification
                {
                    Val = JustificationValues.Right,
                }),
                new Run(new Text(DateTime.Today.ToShortDateString())));
        }

        private static Paragraph Betreff(Vertrag Vertrag, DateTime Abrechnungsbeginn, DateTime Abrechnungsende)
        {
            var Nutzungsbeginn = Abrechnungsbeginn > Vertrag.Beginn ? Abrechnungsbeginn : Vertrag.Beginn;
            var Nutzungsende = Vertrag.Ende is DateTime vertragsEnde && Abrechnungsende > Vertrag.Ende
                ? vertragsEnde : Abrechnungsende;

            var Mieterliste = string.Join(", ",
                Vertrag.Mieter.Select(m => m.Kontakt.Vorname + " " + m.Kontakt.Nachname));

            return new Paragraph(
                new Run(
                    new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                    new Text("Betriebskostenabrechnung 2018"), // TODO 2018 is hard coded.
                    new Break()),
                new Run(
                    new Text("Mieter: " + Mieterliste),
                    new Break(),
                    new Text("Mietobjekt: " + Vertrag.Wohnung!.Adresse.Strasse + " " +
                        Vertrag.Wohnung.Adresse.Hausnummer + ", " + Vertrag.Wohnung.Bezeichnung),
                    new Break(),
                    new Text("Abrechnungszeitraum: "),
                    new TabChar(),
                    new Text(Abrechnungsbeginn.ToShortDateString() + " - " + Abrechnungsende.ToShortDateString()),
                    new Break(),
                    new Text("Nutzungszeitraum: "),
                    new TabChar(),
                    new Text(Nutzungsbeginn.ToShortDateString() + " - " + Nutzungsende.ToShortDateString())));
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

            return new Paragraph(
                new Run(
                    new Text(Gruss),
                    new Break(),
                    new Text("wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. " +
                        resultTxt1),
                    new TabChar()),
                new Run(
                    new RunProperties(
                        new Bold() { Val = OnOffValue.FromBoolean(true), },
                        new Underline() { Val = UnderlineValues.Single, }),
                    new Text(string.Format("{0:N2}€", result)),
                    new Break()),
                result > 0 ? refund : demand);
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
            return new Table(
                // This cell defines the width of the left cell.
                new TableRow(
                    ContentHead("1000", "Umlageschlüssel"),
                    ContentHead("Bedeutung")),
                new TableRow(
                    ContentCell("n. WF."),
                    ContentCell("nach Wohn-/Nutzfläche in m²")),
                new TableRow(
                    ContentCell("n. NE."),
                    ContentCell("nach Anzahl der Wohn-/Nutzeinheiten")),
                new TableRow(
                    ContentCell("n. Pers."),
                    ContentCell("nach Personenzahl/Anzahl der Bewohner")),
                new TableRow( // This row has SpacingBeetweenLine.
                    ContentCellEnd("n. Verb."),
                    ContentCellEnd("nach Verbrauch (in m³ oder in kWh)")),

                new TableRow(
                    new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text("Umlageweg")))),
                    new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text("Beschreibung"))))),
                new TableRow(
                    ContentCell("n. WF."),
                    ContentCell("Kostenanteil = Kosten je Quadratmeter Wohn-/Nutzfläche mal Anteil Fläche je Wohnung.")),
                new TableRow(
                    ContentCell("n. NE."),
                    ContentCell("Kostenanteil = Kosten je Wohn-/Nutzeinheit.")),
                new TableRow(
                    ContentCell("n. Pers."),
                    ContentCell("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung.")),
                new TableRow( // This row has SpacingBeetweenLine.
                    ContentCellEnd("n. Verb."),
                    ContentCellEnd("Kostenanteil = Kosten je Verbrauchseinheit mal individuelle Verbrauchsmenge in Kubikmetern oder Kilowattstunden.")),

                new TableRow(
                    ContentCellEnd("Anmerkung: "),
                    ContentCellEnd("Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger)Kostenanteil")));
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

        private static Table Abrechnungseinheit(Adresse Adresse, DateTime Abrechnungsbeginn, DateTime Abrechnungsende)
        {
            var vertraege = Adresse.Wohnungen.SelectMany(w => w.Vertraege.Where(v =>
                v.Beginn <= Abrechnungsende && (v.Ende is null || v.Ende >= Abrechnungsbeginn)));

            var table = new Table(
                new TableRow(
                    ContentHeadCenter("1050", "Objekt"),
                    ContentHeadCenter("620", "Wohn-/Nutz- einheiten"),
                    ContentHeadCenter("565", "Wohnfläche in m²"),
                    ContentHeadCenter("675", "Nutzfläche Heizung in m²"),
                    ContentHeadCenter("480", "Haus- bewohner"),
                    ContentHeadCenter("1120", "Nutzungsintervall"),
                    ContentHeadCenter("480", "Tage")));

            var Wohnflaeche = Adresse.Wohnungen.Sum(w => w.Wohnflaeche);
            var Nutzflaeche = Adresse.Wohnungen.Sum(w => w.Nutzflaeche);
            var Nutzeinheit = Adresse.Wohnungen.Count();
            var Totaltimespan = ((Abrechnungsende - Abrechnungsbeginn).Days + 1).ToString();

            var intervals = VertraegeIntervallPersonenzahl(vertraege, Abrechnungsbeginn, Abrechnungsende).ToList();
            for (var i = 0; i < intervals.Count(); i++)
            {
                var (beginn, personenzahl) = intervals[i];
                var f = beginn == Abrechnungsbeginn;
                var endDate = i + 1 < intervals.Count() ? intervals[i + 1].beginn.AddDays(-1) : Abrechnungsende;

                var timespan = ((endDate - beginn).Days + 1).ToString();

                table.Append(new TableRow( // TODO check for duplicates...
                    ContentCellCenter(f ? Adresse.Strasse + " " + Adresse.Hausnummer : ""),
                    ContentCellCenter(f ? Nutzeinheit.ToString() : ""),
                    ContentCellCenter(f ? Wohnflaeche.ToString() : ""),
                    ContentCellCenter(f ? Nutzflaeche.ToString() : ""),
                    ContentCellCenter(personenzahl.ToString()),
                    ContentCellCenter(beginn.ToShortDateString() + " - " + endDate.ToShortDateString()),
                    ContentCellCenter(timespan + "/" + Totaltimespan)));
            };

            return table;
        }

        private static Table Abrechnungswohnung(Vertrag Vertrag, Wohnung Wohnung, DateTime Abrechnungsbeginn, DateTime Abrechnungsende)
        {
            var vertraege = Wohnung.Vertraege.Where(v => v.VertragId == Vertrag.VertragId);

            var table = new Table(new TableRow(
                ContentHeadCenter("1050", "Ihre Wohnung"),
                ContentHeadCenter("620", "Ihre Nutz- einheiten"),
                ContentHeadCenter("565", "Ihre Wohn- fläche"),
                ContentHeadCenter("675", "Ihre Nutzfläche"),
                ContentHeadCenter("480", "Anzahl Bewohner"),
                ContentHeadCenter("1120", "Nutzungsintervall"),
                ContentHeadCenter("480", "Tage")));

            var Totaltimespan = ((Abrechnungsende - Abrechnungsbeginn).Days + 1).ToString();

            var f = true;
            foreach (var vertrag in vertraege)
            {
                var endDate = Min(vertrag.Ende ?? Abrechnungsende, Abrechnungsende);
                var beginDate = Max(vertrag.Beginn, Abrechnungsbeginn);

                var timespan = ((endDate - beginDate).Days + 1).ToString();

                table.Append(new TableRow( 
                    ContentCellCenter(f ? Wohnung.Bezeichnung : ""),
                    ContentCellCenter(f ? 1.ToString() : ""), // TODO  ... 1 ? hmm...
                    ContentCellCenter(f ? Wohnung.Wohnflaeche.ToString() : ""),
                    ContentCellCenter(f ? Wohnung.Nutzflaeche.ToString() : ""),
                    ContentCellCenter(Vertrag.Personenzahl.ToString()),
                    ContentCellCenter(beginDate.ToShortDateString() + " - " + endDate.ToShortDateString()),
                    ContentCellCenter(timespan + "/" + Totaltimespan)));

                f = false;
            };

            return table;
        }

        private static Table ErmittlungEinheiten(Vertrag Vertrag, Wohnung Wohnung, DateTime Abrechnungsbeginn, DateTime Abrechnungsende)
        {
            var Wohnflaeche = Wohnung.Adresse.Wohnungen.Sum(w => w.Wohnflaeche);
            var Nutzeinheit = Wohnung.Adresse.Wohnungen.Count();

            var Vertraege = Wohnung.Vertraege.Where(v => v.VertragId == Vertrag.VertragId).OrderBy(v => v.Beginn);
            var Beginn = Max(Vertraege.First().Beginn, Abrechnungsbeginn);
            var Ende = Min(Vertraege.Last().Ende ?? Abrechnungsende, Abrechnungsende);

            var Nutzungszeitraum = ((Ende - Beginn).Days + 1);
            var Totaltimespan = ((Abrechnungsende - Abrechnungsbeginn).Days + 1);
            var Zeitanteil = (double)Nutzungszeitraum / (double)Totaltimespan;
            var WFZeitanteil = (Wohnung.Wohnflaeche / Wohnflaeche) * Zeitanteil;
            var NEZeitanteil = (1.0 / Nutzeinheit) * Zeitanteil;

            var table = new Table(
                new TableRow(ContentHead("2000", "Ermittlung Ihrer Einheiten")),
                new TableRow(
                    ContentHead("bei Umlage nach Wohnfläche (n. WF)"),
                    ContentHeadCenter("1620", "Nutzungsintervall"),
                    ContentHeadCenter("480", "Tage"),
                    ContentHeadCenter("900", "Ihr Anteil")),
                new TableRow(
                    ContentCellCenter(Wohnung.Wohnflaeche.ToString() + " / " + Wohnflaeche.ToString()),
                    ContentCellCenter(Beginn.ToShortDateString() + " - " + Ende.ToShortDateString()),
                    ContentCellCenter(Nutzungszeitraum.ToString() + " / " + Totaltimespan.ToString()),
                    ContentCellCenter(Percent(WFZeitanteil))),
                new TableRow(
                    ContentHead("bei Umlage nach Nutzeinheiten (n. NE)")),
                new TableRow(
                    ContentCellCenter(1.ToString() + " / " + Nutzeinheit),
                    ContentCellCenter(Beginn.ToShortDateString() + " - " + Ende.ToShortDateString()),
                    ContentCellCenter(Nutzungszeitraum.ToString() + " / " + Totaltimespan.ToString()),
                    ContentCellCenter(Percent(NEZeitanteil))),
                new TableRow(
                    ContentHead("bei Umlage nach Personenzahl (n. Pers.)")));
            
            foreach (var v in Vertraege)
            {
                var Personenzahl = Wohnung.Adresse.Wohnungen
                    .SelectMany(w => w.Vertraege)
                    .Where(v => v.Ende > Vertrag.Beginn && Vertrag.Ende is null || v.Beginn < Vertrag.Ende)
                    .Sum(a => a.Personenzahl);

                var vBegin = Max(v.Beginn, Abrechnungsbeginn);
                var vEnd = Min(v.Ende ?? Abrechnungsende, Abrechnungsende);
                var vZeitraum = ((vEnd - vBegin).Days + 1);
                var PersZeitanteil = ((double)v.Personenzahl / (double)Personenzahl) * ((double)vZeitraum / (double)Totaltimespan);

                table.Append(new TableRow(
                    ContentCellCenter(v.Personenzahl.ToString() + " / " + Personenzahl.ToString()),
                    ContentCellCenter(vBegin.ToShortDateString() + " - " + vEnd.ToShortDateString()),
                    ContentCellCenter(vZeitraum.ToString() + " / " + Totaltimespan.ToString()),
                    ContentCellCenter(Percent(PersZeitanteil))));
            }

            return table;
        }

        private static Table ErmittlungKosten(Vertrag Vertrag, Wohnung Wohnung, DateTime Abrechnungsbeginn, DateTime Abrechnungsende, int Jahr)
        {
            var Wohnflaeche = Wohnung.Adresse.Wohnungen.Sum(w => w.Wohnflaeche);
            var Nutzeinheit = Wohnung.Adresse.Wohnungen.Count();

            var Vertraege = Wohnung.Vertraege.Where(v => v.VertragId == Vertrag.VertragId).OrderBy(v => v.Beginn);
            var Beginn = Max(Vertraege.First().Beginn, Abrechnungsbeginn);
            var Ende = Min(Vertraege.Last().Ende ?? Abrechnungsende, Abrechnungsende);
            var Nutzungszeitraum = ((Ende - Beginn).Days + 1);
            var Totaltimespan = ((Abrechnungsende - Abrechnungsbeginn).Days + 1);
            var Zeitanteil = (double)Nutzungszeitraum / (double)Totaltimespan;
            var WFZeitanteil = (Wohnung.Wohnflaeche / Wohnflaeche) * Zeitanteil;
            var NEZeitanteil = (1.0 / Nutzeinheit) * Zeitanteil;

            var kalteBetriebskosten = Wohnung.Adresse.KalteBetriebskosten.OrderBy(k => k.Bezeichnung);
            var Rechnungen = kalteBetriebskosten.SelectMany(k => k.Rechnungen).Where(r => r.Jahr == Jahr);

            var table = new Table(
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color="888888" })),
                new TableRow(
                    ContentHeadCenter("1700", "Kostenanteil"),
                    ContentHead("500", "Schlüssel"),
                    ContentHeadCenter("1120", "Nutzungsintervall"),
                    ContentHeadCenter("500", "Betrag"),
                    ContentHeadCenter("630", "Ihr Anteil"),
                    ContentHeadRight("550", "Ihre Kosten")));

            var ihreSumme = 0.0;
            TableRow kostenPunkt(KalteBetriebskostenpunkt punkt, string zeitraum, int Jahr, double anteil, bool f = true)
            {
                var rechnung = punkt.Rechnungen.Where(r => r.Jahr == Jahr);
                var betrag = rechnung.Count() == 1 ? rechnung.First().Betrag : 0.0;

                ihreSumme += betrag * anteil;

                return new TableRow(
                    ContentCell(f ? punkt.Bezeichnung.ToDescriptionString() : ""),
                    ContentCell(f ? punkt.Schluessel.ToDescriptionString() : ""),
                    ContentCellCenter(f ? zeitraum : ""),
                    ContentCellRight(f ? string.Format("{0:N2}€", betrag) : ""),
                    ContentCellRight(Percent(anteil)),
                    ContentCellRight(string.Format("{0:N2}€", betrag * anteil)));
            }

            foreach (var pt in kalteBetriebskosten)
            {
                if (pt.Schluessel == UmlageSchluessel.NachPersonenzahl)
                {
                    var first = true;
                    foreach (var v in Vertraege)
                    {
                        var Personenzahl = Wohnung.Adresse.Wohnungen
                            .SelectMany(w => w.Vertraege)
                            .Where(v => v.Ende > Vertrag.Beginn && Vertrag.Ende is null || v.Beginn < Vertrag.Ende)
                            .Sum(a => a.Personenzahl);

                        var vBegin = Max(v.Beginn, Abrechnungsbeginn);
                        var vEnd = Min(v.Ende ?? Abrechnungsende, Abrechnungsende);
                        var vZeitraum = ((vEnd - vBegin).Days + 1);

                        var zeitraum = vBegin.ToShortDateString() + " - " + vEnd.ToShortDateString();
                        var anteil = ((double)v.Personenzahl / (double)Personenzahl) * ((double)vZeitraum / (double)Totaltimespan);

                        table.Append(kostenPunkt(pt, zeitraum, Jahr, anteil, first));
                        first = false;
                    }
                }
                else if (pt.Schluessel == UmlageSchluessel.NachWohnflaeche)
                {
                    var zeitraum = Beginn.ToShortDateString() + " - " + Ende.ToShortDateString();
                    table.Append(kostenPunkt(pt, zeitraum, Jahr, WFZeitanteil));
                }
                else if (pt.Schluessel == UmlageSchluessel.NachNutzeinheit)
                {
                    var zeitraum = Beginn.ToShortDateString() + " - " + Ende.ToShortDateString();
                    table.Append(kostenPunkt(pt, zeitraum, Jahr, NEZeitanteil));
                }
            }

            table.Append(new TableRow(
                ContentCell(""), ContentCell(""),
                ContentHeadRight("Summe Gesamtkosten: "),
                ContentHeadRight(string.Format("{0:N2}€", Rechnungen.Sum(r => r.Betrag))),
                ContentHeadRight("Ihre Summe: "),
                ContentHeadRight(string.Format("{0:N2}€", ihreSumme))));

            return table;
        }

        private static string Percent(double d) => string.Format("{0:N2}%", d * 100);

        private static T Max<T>(T l, T r) where T : IComparable<T>
            => Max(l, r, Comparer<T>.Default);
        private static T Max<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) < 0 ? r : l;

        private static T Min<T>(T l, T r) where T : IComparable<T>
            => Min(l, r, Comparer<T>.Default);
        private static T Min<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) > 0 ? r : l;

        private static List<(DateTime beginn, int personenzahl)> VertraegeIntervallPersonenzahl(IEnumerable<Vertrag> vertraege, DateTime Abrechnungsbeginn, DateTime Abrechnungsende)
        {
            var merged = vertraege
                .Where(v => v.Beginn <= Abrechnungsende && (v.Ende is null || v.Ende >= Abrechnungsbeginn))
                .SelectMany(v => new[]
                {
                    (Max(v.Beginn, Abrechnungsbeginn), v.Personenzahl),
                    (Min(v.Ende ?? Abrechnungsende, Abrechnungsende).AddDays(1), -v.Personenzahl)
                })
                .GroupBy(t => t.Item1)
                .Select(g => (beginn: g.Key, personenzahl: g.Sum(t => t.Item2)))
                .OrderBy(t => t.beginn)
                .ToList();
            merged.RemoveAt(merged.Count - 1);

            for (int i = 1, count = merged.Count; i < count; ++i)
            {
                merged[i] = (merged[i].beginn, merged[i - 1].personenzahl + merged[i].personenzahl);
            }

            return merged;
        }

        static RunProperties Bold() => new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
        static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });

        static TableCell ContentCell(string str) => new TableCell(new Paragraph(NoSpace(), new Run(new Text(str))));
        static TableCell ContentHead(string str) => new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text(str))));
        static TableCell ContentHead(string pct, string str)
        {
            return new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(NoSpace(), new Run(Bold(), new Text(str))));
        }
        static TableCell ContentCellEnd(string str) => new TableCell(new Paragraph(new Run(new Text(str))));


        // TODO overload some of this?
        static TableCell ContentCellCenter(string str)
        {
            return new TableCell(
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(new Text(str))));
        }

        static TableCell ContentCellRight(string str)
        {
            return new TableCell(
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(new Text(str))));
        }

        static TableCell ContentHeadCenter(string str)
        {
            return new TableCell(
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(Bold(), new Text(str))));
        }
        static TableCell ContentHeadCenter(string pct, string str)
        {
            return new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(Bold(), new Text(str))));
        }
        static TableCell ContentHeadRight(string str)
        {
            return new TableCell(
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(Bold(), new Text(str))));
        }
        static TableCell ContentHeadRight(string pct, string str)
        {
            return new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(Bold(), new Text(str))));
        }
    }
}
