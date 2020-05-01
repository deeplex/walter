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
                .Include(v => v.Wohnung!)
                    .ThenInclude(w => w.Adresse)
                        .ThenInclude(a => a.Wohnungen)
                            .ThenInclude(w2 => w2.Vertraege)
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
                Abrechnungswohnung(vertrag, vertrag.Wohnung, Abrechnungsbeginn, Abrechnungsende)
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

        private static Paragraph Betreff(Vertrag Vertrag, DateTime Abrechnungsbeginn, DateTime Abrechnungsende)
        {
            var Mieter = Vertrag.Mieter;
            var Adresse = Vertrag.Wohnung!.Adresse;

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
            static RunProperties Bold() => new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
            static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
            static TableCell Content(string str) => new TableCell(new Paragraph(NoSpace(), new Run(new Text(str))));
            static TableCell ContentEnd(string str) => new TableCell(new Paragraph(new Run(new Text(str))));

            return new Table(
                // This cell defines the width of the left cell.
                new TableRow(
                    new TableCell(
                        new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = "1000" }), // 1000% / 50 = 20%
                        new Paragraph(NoSpace(), new Run(Bold(), new Text("Umlageschlüssel")))),
                    new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text("Bedeutung"))))),
                new TableRow(
                    Content("n. WF."),
                    Content("nach Wohn-/Nutzfläche in m²")),
                new TableRow(
                    Content("n. NE."),
                    Content("nach Anzahl der Wohn-/Nutzeinheiten")),
                new TableRow(
                    Content("n. Pers."),
                    Content("nach Personenzahl/Anzahl der Bewohner")),
                new TableRow( // This row has SpacingBeetweenLine.
                    ContentEnd("n. Verb."),
                    ContentEnd("nach Verbrauch (in m³ oder in kWh)")),

                new TableRow(
                    new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text("Umlageweg")))),
                    new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text("Beschreibung"))))),
                new TableRow(
                    Content("n. WF."),
                    Content("Kostenanteil = Kosten je Quadratmeter Wohn-/Nutzfläche mal Anteil Fläche je Wohnung.")),
                new TableRow(
                    Content("n. NE."),
                    Content("Kostenanteil = Kosten je Wohn-/Nutzeinheit.")),
                new TableRow(
                    Content("n. Pers."),
                    Content("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung.")),
                new TableRow( // This row has SpacingBeetweenLine.
                    ContentEnd("n. Verb."),
                    ContentEnd("Kostenanteil = Kosten je Verbrauchseinheit mal individuelle Verbrauchsmenge in Kubikmetern oder Kilowattstunden.")),

                new TableRow(
                    ContentEnd("Anmerkung: "),
                    ContentEnd("Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger)Kostenanteil")));
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
            static RunProperties Bold() => new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
            static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
            static TableCell Content(string str)
            {
                return new TableCell(
                    new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                    new Run(new Text(str))));
            }
            static TableCell HeaderCell(string pct, string str)
            {
                return new TableCell(
                    new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                    new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                    new Run(Bold(), new Text(str))));
            }

            var vertraege = Adresse.Wohnungen.SelectMany(w => w.Vertraege.Where(v =>
                v.Beginn <= Abrechnungsende && (v.Ende is null || v.Ende >= Abrechnungsbeginn)));

            var table = new Table(
                new TableRow(
                    HeaderCell("1050", "Objekt"),
                    HeaderCell("620", "Wohn-/Nutz- einheiten"),
                    HeaderCell("565", "Wohnfläche in m²"),
                    HeaderCell("675", "Nutzfläche Heizung in m²"),
                    HeaderCell("480", "Haus- bewohner"),
                    HeaderCell("1120", "Nutzungsintervall"),
                    HeaderCell("480", "Tage")));

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
                    Content(f ? Adresse.Strasse + " " + Adresse.Hausnummer : ""),
                    Content(f ? Nutzeinheit.ToString() : ""),
                    Content(f ? Wohnflaeche.ToString() : ""),
                    Content(f ? Nutzflaeche.ToString() : ""),
                    Content(personenzahl.ToString()),
                    Content(beginn.ToShortDateString() + " - " + endDate.ToShortDateString()),
                    Content(timespan + "/" + Totaltimespan)));
            };

            return table;
        }

        private static Table Abrechnungswohnung(Vertrag Vertrag, Wohnung Wohnung, DateTime Abrechnungsbeginn, DateTime Abrechnungsende)
        {
            static RunProperties Bold() => new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
            static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
            static TableCell Content(string str)
            {
                return new TableCell(
                    new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                    new Run(new Text(str))));
            }
            static TableCell HeaderCell(string pct, string str)
            {
                return new TableCell(
                    new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                    new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                    new Run(Bold(), new Text(str))));
            }

            var vertraege = Wohnung.Vertraege.Where(v => v.VertragId == Vertrag.VertragId);

            var table = new Table(
                new TableRow(
                    HeaderCell("1050", "Ihre Wohnung"),
                    HeaderCell("620", "Ihre Nutz- einheiten"),
                    HeaderCell("565", "Ihre Wohn- fläche"),
                    HeaderCell("675", "Ihre Nutzfläche"),
                    HeaderCell("480", "Anzahl Bewohner"),
                    HeaderCell("1120", "Nutzungsintervall"),
                    HeaderCell("480", "Tage")));

            var Totaltimespan = ((Abrechnungsende - Abrechnungsbeginn).Days + 1).ToString();
            
            var f = true;
            foreach (var vertrag in vertraege)
            {
                var endDate = Min(vertrag.Ende ?? Abrechnungsende, Abrechnungsende);
                var beginDate = Max(vertrag.Beginn, Abrechnungsbeginn);

                var timespan = ((endDate - beginDate).Days + 1).ToString();

                table.Append(new TableRow( // TODO check for duplicates...
                    Content(f ? Wohnung.Bezeichnung : ""),
                    Content(f ? 1.ToString() : ""), // TODO  ... 1 ? hmm...
                    Content(f ? Wohnung.Wohnflaeche.ToString() : ""),
                    Content(f ? Wohnung.Nutzflaeche.ToString() : ""),
                    Content(Vertrag.Personenzahl.ToString()),
                    Content(beginDate.ToShortDateString() + " - " + endDate.ToShortDateString()),
                    Content(timespan + "/" + Totaltimespan)));

                f = false;
            };

            return table;
        }

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
    }
}
