using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Linq;
using static Deeplex.Saverwalter.Model.Betriebskostenabrechnung;

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
                throw;
            }
            wordDocument.MainDocumentPart.Document.AppendChild(body);
        }

        public static void SaveAsDocx(this Betriebskostenabrechnung b, string filepath)
        {
            var body = new Body(new SectionProperties(
                // Margins after DIN5008
                new PageMargin() { Left = 1418, Right = 567, Top = 958, Bottom = 958, },
                // DIN A4
                new PageSize() { Code = 9, Width = 11906, Height = 16838 }));
            body.Append(
                // p.1
                AnschriftVermieter(b),
                PostalischerVermerk(b),
                PrintDate(),
                Betreff(b),
                Ergebnis(b),
                GenericText(),
                new Break() { Type = BreakValues.Page },
                // p.2
                Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)"),
                ExplainUmlageschluessel(),
                Heading("Erläuterungen zu einzelnen Betriebskostenarten"),
                ExplainKalteBetriebskosten(b),
                new Break() { Type = BreakValues.Page },
                // p.3
                Heading("Abrechnung der Nebenkosten (kalten Betriebskosten)"),
                MietHeader(b));
                
            foreach (var gruppe in b.Gruppen)
            {
                body.Append(
                    SubHeading("Angaben zur Abrechnungseinheit"),
                    Abrechnungseinheit(b, gruppe),
                    Abrechnungswohnung(b, gruppe),
                    new Paragraph(), // Necessary to split tables...
                    ErmittlungEinheiten(b, gruppe),
                    SubHeading("Ermittlung der kalten Betriebskosten"),
                    ErmittlungKosten(b, gruppe));
            }
            body.Append(
                Heading("Gesamtergebnis der Abrechnung"),
                GesamtErgebnis(b));

            CreateWordDocument(filepath, body);
        }

        private static Table AnschriftVermieter(Betriebskostenabrechnung b)
        {
            var table = new Table(
                new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                new TableRow(
                    ContentCell(b.Vermieter.Bezeichnung),
                    ContentCell("", JustificationValues.Right)),
                new TableRow(
                    ContentCell("℅ " + b.Ansprechpartner.Vorname + " " + b.Ansprechpartner.Nachname),
                    ContentCell("Tel.: " + b.Ansprechpartner.Telefon, JustificationValues.Right)),
                new TableRow(
                    ContentCell(b.Ansprechpartner.Adresse!.Strasse + " " + b.Ansprechpartner.Adresse.Hausnummer),
                    ContentCell("Fax: " + b.Ansprechpartner.Fax, JustificationValues.Right)),
                new TableRow(
                    ContentCell(b.Ansprechpartner.Adresse.Postleitzahl + " " + b.Ansprechpartner.Adresse.Stadt),
                    ContentCell("E-Mail: " + b.Ansprechpartner.Email, JustificationValues.Right)));
            table.Append(Enumerable.Range(0, 9 - 4).Select(_ => new Break()));

            return table;
        }

        private static Paragraph PostalischerVermerk(Betriebskostenabrechnung b)
        {
            // TODO We have problems if there are more than 4 Mieter...

            var run = new Run();
            int counter = 6;

            foreach (var m in b.Mieter)
            {
                var Anrede = m.Anrede == Model.Anrede.Herr ? "Herrn " : m.Anrede == Model.Anrede.Frau ? "Frau " : "";
                run.Append(new Text(Anrede + m.Vorname + " " + m.Nachname));
                run.Append(new Break());
                counter--;
            }
            var a = b.Mieter.First().Adresse;
            if (a != null)
            {
                run.Append(new Text(a.Strasse + " " + a.Hausnummer));
                run.Append(new Break());
                counter--;
                run.Append(new Text(a.Postleitzahl + " " + a.Stadt));
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
                new Run(new Text(Datum(DateTime.Today))));
        }

        private static Paragraph Betreff(Betriebskostenabrechnung b)
        {
            var Mieterliste = string.Join(", ",
                b.Mieter.Select(m => m.Vorname + " " + m.Nachname));

            return new Paragraph(
                new Run(
                    new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                    new Text("Betriebskostenabrechnung " + b.Jahr.ToString()), 
                    new Break()),
                new Run(
                    new Text("Mieter: " + Mieterliste),
                    new Break(),
                    new Text("Mietobjekt: " + b.Adresse.Strasse + " " +
                        b.Adresse.Hausnummer + ", " + b.Wohnung.Bezeichnung),
                    new Break(),
                    new Text("Abrechnungszeitraum: "),
                    new TabChar(),
                    new Text(Datum(b.Abrechnungsbeginn) + " - " + Datum(b.Abrechnungsende)),
                    new Break(),
                    new Text("Nutzungszeitraum: "),
                    new TabChar(),
                    new Text(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende))));
        }

        private static Paragraph Ergebnis(Betriebskostenabrechnung b)
        {
            var gruss = b.Mieter.Aggregate("", (r, m) => r + (
                m.Anrede == Anrede.Herr ? "sehr geehrter Herr " :
                m.Anrede == Anrede.Frau ? "sehr geehrte Frau " :
                m.Vorname) + m.Nachname + ", ");
            // Capitalize first letter...
            var Gruss = gruss.Remove(1).ToUpper() + gruss.Substring(1);

            var resultTxt1 = "Die Abrechnung schließt mit " +
                (b.Result > 0 ? "einem Guthaben" : "einer Nachforderung") +
                " in Höhe von: ";

            var refund = new Run(
                new Text("Dieser Betrag wird über die von Ihnen angegebene Bankverbindung erstattet."));

            var demand = new Run(new Text("Bitte überweisen Sie diesen Betrag auf das Ihnen bekannte Konto."));

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
                    new Text(Euro(Math.Abs(b.Result))),
                    new Break()),
                b.Result > 0 ? refund : demand);
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

        private static Paragraph ExplainKalteBetriebskosten(Betriebskostenabrechnung b)
        {
            var para = new Paragraph();

            return para;
        }

        private static Paragraph MietHeader(Betriebskostenabrechnung b)
        {
            var Header = string.Join(", ", b.Mieter.Select(m => (
                m.Anrede == Anrede.Herr ? "Herrn " :
                m.Anrede == Anrede.Frau ? "Frau " :
                m.Vorname) + " " + m.Nachname)) +
                " (" + b.Adresse.Strasse + " " + b.Adresse.Hausnummer + ", " + b.Wohnung.Bezeichnung + ")";

            return new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(new Text(Header)));
        }

        private static Table Abrechnungseinheit(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(
                new TableRow(
                    ContentHead("1050", "Objekt", JustificationValues.Center),
                    ContentHead("620", "Wohn-/Nutz- einheiten", JustificationValues.Center),
                    ContentHead("565", "Wohnfläche in m²", JustificationValues.Center),
                    ContentHead("675", "Nutzfläche Heizung in m²", JustificationValues.Center),
                    ContentHead("480", "Haus- bewohner", JustificationValues.Center),
                    ContentHead("1120", "Nutzungsintervall", JustificationValues.Center),
                    ContentHead("480", "Tage", JustificationValues.Center)));

            for (var i = 0; i < g.GesamtPersonenIntervall.Count(); ++i)
            {
                var (Beginn, Ende, Personenzahl) = g.GesamtPersonenIntervall[i];
                var f = Beginn.Date == b.Abrechnungsbeginn.Date;

                var timespan = ((Ende - Beginn).Days + 1).ToString();

                table.Append(new TableRow( // TODO check for duplicates...
                    ContentCell(f ? b.Adresse.Strasse + " " + b.Adresse.Hausnummer : "", JustificationValues.Center),
                    ContentCell(f ? g.GesamtEinheiten.ToString() : "", JustificationValues.Center),
                    ContentCell(f ? g.GesamtWohnflaeche.ToString() : "", JustificationValues.Center),
                    ContentCell(f ? g.GesamtNutzflaeche.ToString() : "", JustificationValues.Center),
                    ContentCell(Personenzahl.ToString(), JustificationValues.Center),
                    ContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center),
                    ContentCell(timespan + "/" + b.Abrechnungszeitspanne, JustificationValues.Center)));
            };

            return table;
        }

        private static Table Abrechnungswohnung(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(new TableRow(
                ContentHead("1050", "Ihre Wohnung", JustificationValues.Center),
                ContentHead("620", "Ihre Nutz- einheiten", JustificationValues.Center),
                ContentHead("565", "Ihre Wohn- fläche", JustificationValues.Center),
                ContentHead("675", "Ihre Nutzfläche", JustificationValues.Center),
                ContentHead("480", "Anzahl Bewohner", JustificationValues.Center),
                ContentHead("1120", "Nutzungsintervall", JustificationValues.Center),
                ContentHead("480", "Tage", JustificationValues.Center)));

            for (var i = 0; i < g.PersonenIntervall.Count(); ++i)
            {
                var (Beginn, Ende, Personenzahl) = g.PersonenIntervall[i];
                var f = Beginn.Date == b.Nutzungsbeginn.Date;

                var timespan = ((Ende - Beginn).Days + 1).ToString();

                table.Append(new TableRow(
                    ContentCell(f ? b.Wohnung.Bezeichnung : "", JustificationValues.Center),
                    ContentCell(f ? 1.ToString() : "", JustificationValues.Center), // TODO  ... 1 ? hmm...
                    ContentCell(f ? b.Wohnung.Wohnflaeche.ToString() : "", JustificationValues.Center),
                    ContentCell(f ? b.Wohnung.Nutzflaeche.ToString() : "", JustificationValues.Center),
                    ContentCell(Personenzahl.ToString(), JustificationValues.Center),
                    ContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center),
                    ContentCell(timespan + "/" + b.Abrechnungszeitspanne, JustificationValues.Center)));
            };

            return table;
        }

        private static Table ErmittlungEinheiten(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(
                new TableRow(ContentHead("2000", "Ermittlung Ihrer Einheiten")),
                new TableRow(
                    ContentHead("bei Umlage nach Wohnfläche (n. WF)"),
                    ContentHead("1620", "Nutzungsintervall", JustificationValues.Center),
                    ContentHead("480", "Tage", JustificationValues.Center),
                    ContentHead("900", "Ihr Anteil", JustificationValues.Center)),
                new TableRow(
                    ContentCell(b.Wohnung.Wohnflaeche.ToString() + " / " + g.GesamtWohnflaeche.ToString(), JustificationValues.Center),
                    ContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Center),
                    ContentCell(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center),
                    ContentCell(Percent(g.WFZeitanteil), JustificationValues.Center)),
                new TableRow(
                    ContentHead("bei Umlage nach Nutzeinheiten (n. NE)")),
                new TableRow(
                    ContentCell(1.ToString() + " / " + g.GesamtEinheiten, JustificationValues.Center),
                    ContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Center),
                    ContentCell(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center),
                    ContentCell(Percent(g.NEZeitanteil), JustificationValues.Center)),
                new TableRow(
                    ContentHead("bei Umlage nach Personenzahl (n. Pers.)")));

            for (var i = 0; i < g.PersZeitanteil.Count; ++i)
            {
                var Beginn = g.PersZeitanteil[i].Beginn;
                var Ende = g.PersZeitanteil[i].Ende;
                var GesamtPersonenzahl = g.GesamtPersonenIntervall.Last(gs => gs.Beginn.Date <= g.PersZeitanteil[i].Beginn.Date).Personenzahl;
                var Personenzahl = g.PersonenIntervall.Last(p => p.Beginn.Date <= g.PersZeitanteil[i].Beginn).Personenzahl;
                var timespan = ((Ende - Beginn).Days + 1).ToString();

                table.Append(new TableRow(
                    ContentCell(Personenzahl.ToString() + " / " + GesamtPersonenzahl.ToString(), JustificationValues.Center),
                    ContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center),
                    ContentCell(timespan + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center),
                    ContentCell(Percent(g.PersZeitanteil[i].Anteil), JustificationValues.Center)));
            }

            return table;
        }

        private static Table ErmittlungKosten(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color = "888888" })),
                new TableRow(
                    ContentHead("1700", "Kostenanteil", JustificationValues.Center),
                    ContentHead("450", "Schlüssel"),
                    ContentHead("1120", "Nutzungsintervall", JustificationValues.Center),
                    ContentHead("550", "Betrag", JustificationValues.Center),
                    ContentHead("630", "Ihr Anteil", JustificationValues.Center),
                    ContentHead("550", "Ihre Kosten", JustificationValues.Center)));

            TableRow kostenPunkt(Betriebskostenrechnung rechnung, string zeitraum, int Jahr, double anteil, bool f = true)
            {
                return new TableRow(
                    ContentCell(f ? rechnung.Typ.ToDescriptionString() : ""),
                    ContentCell(f ? rechnung.Schluessel.ToDescriptionString() : ""),
                    ContentCell(zeitraum, JustificationValues.Center),
                    ContentCell(Euro(rechnung.Betrag), JustificationValues.Right), // TODO f ? bold : normal?
                    ContentCell(Percent(anteil), JustificationValues.Right),
                    ContentCell(Euro(rechnung.Betrag * anteil), JustificationValues.Right));
            }

            foreach (var rechnung in g.Rechnungen)
            {
                string zeitraum;
                switch (rechnung.Schluessel)
                {
                    case UmlageSchluessel.NachWohnflaeche:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        table.Append(kostenPunkt(rechnung, zeitraum, b.Jahr, g.WFZeitanteil));
                        break;
                    case UmlageSchluessel.NachNutzeinheit:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        table.Append(kostenPunkt(rechnung, zeitraum, b.Jahr, g.NEZeitanteil));
                        break;
                    case UmlageSchluessel.NachPersonenzahl:
                        var first = true;
                        foreach (var a in g.PersZeitanteil)
                        {
                            zeitraum = Datum(a.Beginn) + " - " + Datum(a.Ende);
                            table.Append(kostenPunkt(rechnung, zeitraum, b.Jahr, a.Anteil, first));
                            first = false;
                        }
                        break;
                    default:
                        break; // TODO or throw something...
                }
            }

            table.Append(new TableRow(
                ContentCell(""), ContentCell(""),
                ContentHead("Summe Gesamtkosten: ", JustificationValues.Right),
                ContentHead(Euro(b.GesamtBetragKalt), JustificationValues.Right),
                ContentHead("Ihre Summe: ", JustificationValues.Right),
                ContentHead(Euro(b.BetragKalt), JustificationValues.Right)));

            return table;
        }

        private static Table GesamtErgebnis(Betriebskostenabrechnung b)
        {
            return new Table(
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color = "888888" })),
                new TableWidth() { Width = "2500", Type = TableWidthUnitValues.Pct },
                new TableRow(
                    ContentCell("Ihre geleisteten Vorauszahlungen:"),
                    ContentCell(Euro(b.Gezahlt), JustificationValues.Right)),
                new TableRow(
                    ContentCell("abzüglich Ihrer Nebenkostenanteile:"),
                    ContentCell(Euro(b.BetragKalt), JustificationValues.Right)),
                new TableRow(
                    ContentCell(b.Result > 0 ? "Erstattungsbetrag:" : "Nachforderungsbetrag:"),
                    ContentHead(Euro(Math.Abs(b.Result)), JustificationValues.Right)));
        }

        // Helper
        private static string Percent(double d) => string.Format("{0:N2}%", d * 100);
        private static string Euro(double d) => string.Format("{0:N2}€", d);
        private static string Datum(DateTime d) => d.ToString("dd.MM.yyyy");

        static RunProperties Bold() => new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
        static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
        static Paragraph SubHeading(string str) => new Paragraph(NoSpace(), new Run(Bold(), new Text(str)));
        static Paragraph Heading(string str)
            => new Paragraph(new Run(new RunProperties(
                new Bold() { Val = OnOffValue.FromBoolean(true) },
                new Italic() { Val = OnOffValue.FromBoolean(true) }),
                new Text(str)));

        static TableCell ContentCell(string str) => new TableCell(new Paragraph(NoSpace(), new Run(new Text(str))));
        static TableCell ContentCell(string str, JustificationValues value)
            => new TableCell(
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(new Text(str))));
        
        static TableCell ContentHead(string str) => new TableCell(new Paragraph(NoSpace(), new Run(Bold(), new Text(str))));
        static TableCell ContentHead(string pct, string str)
            => new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(NoSpace(), new Run(Bold(), new Text(str))));
        static TableCell ContentHead(string str, JustificationValues value)
            => new TableCell(
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(Bold(), new Text(str))));
        static TableCell ContentHead(string pct, string str, JustificationValues value)
            => new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(Bold(), new Text(str))));

        static TableCell ContentCellEnd(string str) => new TableCell(new Paragraph(new Run(new Text(str))));
    }
}
