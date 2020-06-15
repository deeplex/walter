using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Immutable;
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
            var body = new Body(
                new SectionProperties(
                // Margins after DIN5008
                new PageMargin() { Left = 1418, Right = 567, Top = 958, Bottom = 958, },
                // DIN A4
                new PageSize() { Code = 9, Width = 11906, Height = 16838 }));

            body.Append(
                // p.1
                AnschriftVermieter(b),
                EmptyRows(4),
                PostalischerVermerk(b),
                PrintDate(),
                Betreff(b),
                Ergebnis(b),
                GenericText(),
                new Break() { Type = BreakValues.Page },
                // p.2
                Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)"),
                ExplainUmlageschluessel(b),
                Heading("Erläuterungen zu einzelnen Betriebskostenarten"),
                ExplainKalteBetriebskosten(b),
                new Break() { Type = BreakValues.Page },
                // p.3
                SubHeading("Angaben zu Ihrer Einheit:"),
                Abrechnungswohnung(b, b.Gruppen.FirstOrDefault()),
                new Paragraph(NoSpace()),
                Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)"));
                // MietHeader(b), TODO Make this in a heading

            foreach (var gruppe in b.Gruppen)
            {
                if (gruppe.GesamtEinheiten == 1)
                {
                    body.Append(
                        SubHeading("Direkt zugeordnet:"),
                        ErmittlungKalteKosten(b, gruppe, true));
                }
                else
                {
                    body.Append(
                        SubHeading("Angaben zur Abrechnungseinheit:", true),
                        Abrechnungsgruppe(b, gruppe),
                        GruppenVerbrauch(b, gruppe),
                        Abrechnungseinheit(b, gruppe),
                        new Paragraph(NoSpace()),
                        ErmittlungKalteEinheiten(b, gruppe),
                        SubHeading("Ermittlung der kalten Betriebskosten", true),
                        ErmittlungKalteKosten(b, gruppe));
                }
            }
            body.Append(
                Heading("Abrechnung der Nebenkosten (warme Nebenkosten)"));
            foreach (var gruppe in b.Gruppen)
            {
                if (gruppe.GesamtEinheiten == 1)
                {
                    body.Append(
                        SubHeading("Direkt zugeordnet:"),
                        ErmittlungWarmeKosten(b, gruppe, true));
                }
            }

            body.Append(
                new Paragraph(),
                Heading("Gesamtergebnis der Abrechnung"),
                GesamtErgebnis(b));

            CreateWordDocument(filepath, body);
        }

        private static Table AnschriftVermieter(Betriebskostenabrechnung b)
        {
            var AnsprechpartnerBezeichnung = b.Ansprechpartner is NatuerlichePerson a ? a.Vorname + " " + a.Nachname : ""; // TODO jur. Person

            // TODO Ansprechpartner HAS to have a Adresse...
            var table = new Table(
                new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                new TableRow(
                    ContentCell(b.Vermieter.Bezeichnung),
                    ContentCell("", JustificationValues.Right)),
                new TableRow(
                    ContentCell("℅ " + AnsprechpartnerBezeichnung),
                    ContentCell("Tel.: " + b.Ansprechpartner.Telefon, JustificationValues.Right)),
                new TableRow(
                    ContentCell(b.Ansprechpartner.Adresse!.Strasse + " " + b.Ansprechpartner.Adresse.Hausnummer),
                    ContentCell("Fax: " + b.Ansprechpartner.Fax, JustificationValues.Right)),
                new TableRow(
                    ContentCell(b.Ansprechpartner.Adresse.Postleitzahl + " " + b.Ansprechpartner.Adresse.Stadt),
                    ContentCell("E-Mail: " + b.Ansprechpartner.Email, JustificationValues.Right)));

            return table;
        }

        private static Paragraph EmptyRows(int len)
        {
            var p = new Paragraph(Font());
            var r = new Run(Font());
            p.Append(r);
            for (var i = 0; i < len; ++i)
            {
                r.Append(new Break());
            }
            return p;
        }

        private static Paragraph PostalischerVermerk(Betriebskostenabrechnung b)
        {
            // TODO We have problems if there are more than 4 Mieter...

            var run = new Run(Font());
            int counter = 6;

            foreach (var m in b.Mieter)
            {
                // If b.Mieter is a jur. Person, there is no Anrede...
                var Anrede = m.Anrede == Model.Anrede.Herr ? "Herrn " : m.Anrede == Model.Anrede.Frau ? "Frau " : "";
                run.Append(new Text(Anrede + m.Bezeichnung));
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

            return new Paragraph(Font(), run);
        }

        private static Paragraph PrintDate()
        {
            return new Paragraph(new ParagraphProperties(new Justification
            {
                Val = JustificationValues.Right,
            }),
                new Run(Font(), new Text(Datum(DateTime.UtcNow.Date))));
        }

        private static Paragraph Betreff(Betriebskostenabrechnung b)
        {
            var Mieterliste = string.Join(", ", b.Mieter.Select(m => m.Bezeichnung));

            return new Paragraph(Font(),
                new Run(Font(),
                    new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                    new Text("Betriebskostenabrechnung " + b.Jahr.ToString()),
                    new Break()),
                new Run(Font(),
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
            var gruss = b.Mieter.Aggregate("", (r, m) =>
            {
                if (m is NatuerlichePerson n)
                {
                    return (n.Anrede == Anrede.Herr ? "sehr geehrter Herr " :
                        n.Anrede == Anrede.Frau ? "sehr geehrte Frau " :
                        n.Vorname) + n.Nachname + ", ";
                }
                else
                {
                    return "";
                }
            });


            // Capitalize first letter...
            var Gruss = gruss.Remove(1).ToUpper() + gruss.Substring(1);

            var resultTxt1 = "Die Abrechnung schließt mit " +
                (b.Result > 0 ? "einem Guthaben" : "einer Nachforderung") +
                " in Höhe von: ";

            var refund = new Run(Font(),
                new Text("Dieser Betrag wird über die von Ihnen angegebene Bankverbindung erstattet."));

            var demand = new Run(Font(), new Text("Bitte überweisen Sie diesen Betrag auf das Ihnen bekannte Konto."));

            return new Paragraph(Font(),
                new Run(Font(),
                    new Text(Gruss),
                    new Break(),
                    new Text("wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. " +
                        resultTxt1),
                    new TabChar()),
                new Run(Font(),
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
            return new Paragraph(Font(),
                new ParagraphProperties(new Justification() { Val = JustificationValues.Both, }),
                new Run(Font(), new Text("Die Abrechnung betrifft zunächst die mietvertraglich vereinbarten Nebenkosten (die kalten Betriebskosten). Die Kosten für die Heizung und für die Erwärmung von Wasser über die Heizanlage Ihres Wohnhauses(warme Betriebskosten) werden gesondert berechnet, nach Verbrauch und Wohn -/ Nutzfläche auf die einzelnen Wohnungen umgelegt(= „Ihre Heizungsrechnung“) und mit dem Ergebnis aus der Aufrechnung Ihrer Nebenkosten und der Summe der von Ihnen geleisteten Vorauszahlungen verrechnet. Bei bestehenden Mietrückständen ist das Ergebnis der Abrechnung zusätzlich mit den Mietrückständen verrechnet. Gegebenenfalls bestehende Mietminderungen / Ratenzahlungsvereinbarungen sind hier nicht berücksichtigt, haben aber weiterhin für den vereinbarten Zeitraum Bestand. Aufgelöste oder gekündigte Mietverhältnisse werden durch dieses Schreiben nicht neu begründet. Die Aufstellung, Verteilung und Erläuterung der Gesamtkosten, die Berechnung der Kostenanteile, die Verrechnung der geleisteten Vorauszahlungen und gegebenenfalls die Neuberechnung der monatlichen Vorauszahlungen entnehmen Sie bitte den folgenden Seiten.")));
        }

        private static Table ExplainUmlageschluessel(Betriebskostenabrechnung b)
        {
            var dir = b.Gruppen.Any(g => g.Rechnungen.Any(r => r.Gruppen.Count == 1));
            var nWF = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachWohnflaeche));
            var nNE = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachNutzeinheit));
            var nPZ = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachPersonenzahl));
            var nVb = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachVerbrauch));

            var t = new Table(
                // This cell defines the width of the left cell.
                new TableRow(
                    ContentHead("1250", "Umlageschlüssel"),
                    ContentHead("Bedeutung")));

            if (dir == true)
            {
                t.Append(new TableRow(
                    ContentCell("Direkt"),
                    ContentCell("Direkte Zuordnung")));
            }
            if (nWF == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. WF."),
                    ContentCell("nach Wohn-/Nutzfläche in m²")));
            }
            if (nNE == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. NE."),
                    ContentCell("nach Anzahl der Wohn-/Nutzeinheiten")));
            }
            if (nPZ == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. Pers."),
                    ContentCell("nach Personenzahl/Anzahl der Bewohner")));
            }
            if (nVb == true)
            {
                t.Append(new TableRow( // This row has SpacingBeetweenLine.
                    ContentCell("n. Verb."),
                    ContentCell("nach Verbrauch (in m³ oder in kWh)")));
            }

            t.Append(new TableRow(ContentCell(""), ContentCell("")),
                new TableRow(
                    new TableCell(new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text("Umlageweg")))),
                    new TableCell(new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text("Beschreibung"))))));

            if (dir == true)
            {
                t.Append(new TableRow(
                    ContentCell("Direkt"),
                    ContentCell("Kostenanteil = Kosten werden Einheit direkt zugeordnet.")));
            }
            if (nWF == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. WF."),
                    ContentCell("Kostenanteil = Kosten je Quadratmeter Wohn-/Nutzfläche mal Anteil Fläche je Wohnung.")));
            }
            if (nNE == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. NE."),
                    ContentCell("Kostenanteil = Kosten je Wohn-/Nutzeinheit.")));
            }
            if (nPZ == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. Pers."),
                    ContentCell("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung.")));
            }
            if (nVb)
            {
                t.Append(new TableRow( // This row has SpacingBeetweenLine.
                    ContentCellEnd("n. Verb."),
                    ContentCellEnd("Kostenanteil = Kosten je Verbrauchseinheit mal individuelle Verbrauchsmenge in Kubikmetern oder Kilowattstunden.")));
            }

            t.Append(new TableRow(ContentCell(""), ContentCell("")),
                    ContentCellEnd("Anmerkung: "),
                    ContentCellEnd("Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger) Kostenanteil"));

            return t;
        }

        private static Paragraph ExplainKalteBetriebskosten(Betriebskostenabrechnung b)
        {
            var para = new Paragraph(Font());

            return para;
        }

        private static Paragraph MietHeader(Betriebskostenabrechnung b)
        {
            var Header = string.Join(", ", b.Mieter.Select(m =>
            {
                if (m is NatuerlichePerson n)
                {
                    return (n.Anrede == Anrede.Herr ? "Herrn " :
                        n.Anrede == Anrede.Frau ? "Frau " :
                        n.Vorname) + " " + n.Nachname;
                }
                else
                {
                    return "";
                }
            })) + " (" + b.Adresse.Strasse + " " + b.Adresse.Hausnummer + ", " + b.Wohnung.Bezeichnung + ")";

            return new Paragraph(Font(),
                new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(Font(), new Text(Header)));
        }

        private static Paragraph Abrechnungsgruppe(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var p = new Paragraph(Font());
            var adressen = g.Rechnungen.First().Gruppen.Select(w => w.Wohnung).GroupBy(w => w.Adresse);

            foreach (var adr in adressen)
            {
                var a = adr.Key;
                var ret = a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;

                if (adr.Count() != a.Wohnungen.Count)
                {
                    ret += ": " + string.Join(", ", adr.Select(w => w.Bezeichnung));
                }
                else
                {
                    ret += " (gesamt)";
                }

                p.Append(new Run(Font(), new Text(ret)));
                if (a != adressen.Last().Key)
                {
                    p.Append(new Break());
                }
            }

            return p;
        }

        private static Table GruppenVerbrauch(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            return new Table();
        }

        private static Table Abrechnungseinheit(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(
                new TableRow(
                    ContentHead("700", "Nutzeinheiten", JustificationValues.Center),
                    ContentHead("1050", "Wohnfläche", JustificationValues.Center),
                    ContentHead("950", "Nutzfläche", JustificationValues.Center),
                    ContentHead("600", "Bewohner", JustificationValues.Center),
                    ContentHead("1400", "Nutzungsintervall", JustificationValues.Center),
                    ContentHead("300", "Tage", JustificationValues.Center)));

            for (var i = 0; i < g.GesamtPersonenIntervall.Count(); ++i)
            {
                var (Beginn, Ende, Personenzahl) = g.GesamtPersonenIntervall[i];
                var f = Beginn.Date == b.Abrechnungsbeginn.Date;

                var timespan = ((Ende - Beginn).Days + 1).ToString();

                table.Append(new TableRow( // TODO check for duplicates...
                    ContentCell(f ? g.GesamtEinheiten.ToString() : "", JustificationValues.Center),
                    ContentCell(f ? Quadrat(g.GesamtWohnflaeche) : "", JustificationValues.Center),
                    ContentCell(f ? Quadrat(g.GesamtNutzflaeche) : "", JustificationValues.Center),
                    ContentCell(Personenzahl.ToString(), JustificationValues.Center),
                    ContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center),
                    ContentCell(timespan + "/" + b.Abrechnungszeitspanne, JustificationValues.Center)));
            };

            return table;
        }

        private static Table Abrechnungswohnung(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            var table = new Table(new TableRow(
                ContentHead("700", "Nutzeinheiten", JustificationValues.Center),
                ContentHead("1050", "Wohnfläche", JustificationValues.Center),
                ContentHead("950", "Nutzfläche", JustificationValues.Center),
                ContentHead("600", "Bewohner", JustificationValues.Center),
                ContentHead("1400", "Nutzungsintervall", JustificationValues.Center),
                ContentHead("300", "Tage", JustificationValues.Center)));

            if (g == null) return table; // If Gruppen is empty...

            for (var i = 0; i < g.PersonenIntervall.Count(); ++i)
            {
                var (Beginn, Ende, Personenzahl) = g.PersonenIntervall[i];
                var f = Beginn.Date == b.Nutzungsbeginn.Date;

                var timespan = ((Ende - Beginn).Days + 1).ToString();

                table.Append(new TableRow(
                    ContentCell(f ? 1.ToString() : "", JustificationValues.Center), // TODO  ... 1 ? hmm...
                    ContentCell(f ? Quadrat(b.Wohnung.Wohnflaeche) : "", JustificationValues.Center),
                    ContentCell(f ? Quadrat(b.Wohnung.Nutzflaeche) : "", JustificationValues.Center),
                    ContentCell(Personenzahl.ToString(), JustificationValues.Center),
                    ContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center),
                    ContentCell(timespan + "/" + b.Abrechnungszeitspanne, JustificationValues.Center)));
            };

            return table;
        }

        private static Table ErmittlungKalteEinheiten(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            TableCell IContentCell(string str, JustificationValues value, BorderType border) =>
                new TableCell(new TableCellProperties(border),
                    new Paragraph(new ParagraphProperties(new Justification() { Val = value }),
                    Font(), NoSpace(), new Run(Font(), new Text(str))));

            BottomBorder bot() => new BottomBorder() { Val = BorderValues.Single, Size = 4 };
            TopBorder top() => new TopBorder() { Val = BorderValues.Single, Size = 4 };
            TableRow empty() => new TableRow(ContentCell(""), ContentCell(""), ContentCell(""), ContentCell(""));

            var table = new Table(new TableRow(
                ContentHead("2050", "Ermittlung Ihrer Einheiten"),
                ContentHead("1120", "Nutzungsintervall", JustificationValues.Center),
                ContentHead("1200", "Tage", JustificationValues.Center),
                ContentHead("630", "Ihr Anteil", JustificationValues.Center)));

            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachWohnflaeche))
            {
                table.Append(
                    new TableRow(ContentHead("bei Umlage nach Wohnfläche (n. WF)"), ContentHead(""), ContentHead(""), ContentHead("")),
                    new TableRow(
                        IContentCell(Quadrat(b.Wohnung.Wohnflaeche) + " / " + Quadrat(g.GesamtWohnflaeche), JustificationValues.Left, bot()),
                        IContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Center, bot()),
                        IContentCell(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center, bot()),
                        IContentCell(Percent(g.WFZeitanteil), JustificationValues.Center, bot())));
            }
            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachNutzeinheit))
            {
                table.Append(
                    new TableRow(ContentHead("bei Umlage nach Nutzeinheiten (n. NE)"), ContentHead(""), ContentHead(""), ContentHead("")),
                    new TableRow(
                        IContentCell(1.ToString() + " / " + g.GesamtEinheiten, JustificationValues.Left, bot()),
                        IContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Center, bot()),
                        IContentCell(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center, bot()),
                        IContentCell(Percent(g.NEZeitanteil), JustificationValues.Center, bot())));
            }
            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachPersonenzahl))
            {
                table.Append(new TableRow(
                    ContentHead("bei Umlage nach Personenzahl (n. Pers.)"), ContentHead(""), ContentHead(""), ContentHead("")));
                string PersonEn(int i) => i.ToString() + (i > 1 ? " Personen" : " Person");
                for (var i = 0; i < g.PersZeitanteil.Count; ++i)
                {
                    var Beginn = g.PersZeitanteil[i].Beginn;
                    var Ende = g.PersZeitanteil[i].Ende;
                    var GesamtPersonenzahl = g.GesamtPersonenIntervall.Last(gs => gs.Beginn.Date <= g.PersZeitanteil[i].Beginn.Date).Personenzahl;
                    var Personenzahl = g.PersonenIntervall.Last(p => p.Beginn.Date <= g.PersZeitanteil[i].Beginn).Personenzahl;
                    var timespan = ((Ende - Beginn).Days + 1).ToString();

                    if (i == g.PersZeitanteil.Count - 1)
                    {
                        table.Append(new TableRow(
                           IContentCell(PersonEn(Personenzahl) + " / " + PersonEn(GesamtPersonenzahl), JustificationValues.Left, bot()),
                           IContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center, bot()),
                           IContentCell(timespan + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center, bot()),
                           IContentCell(Percent(g.PersZeitanteil[i].Anteil), JustificationValues.Center, bot())));
                    }
                    else
                    {
                        table.Append(new TableRow(
                            ContentCell(PersonEn(Personenzahl) + " / " + PersonEn(GesamtPersonenzahl)),
                            ContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center),
                            ContentCell(timespan + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center),
                            ContentCell(Percent(g.PersZeitanteil[i].Anteil), JustificationValues.Center)));
                    }
                }
            }

            if (g.Verbrauch.Count > 0)
            {
                table.Append(new TableRow(
                    ContentHead("bei Umlage nach Verbrauch (n. Verb.)"),
                    ContentHead(""), ContentHead("Zählernummer", JustificationValues.Center), ContentHead("")));
                foreach (var Verbrauch in g.Verbrauch.Where(v => (int)v.Key % 2 == 0)) // Kalte Betriebskosten are equal / warme are odd
                {
                    foreach (var Value in Verbrauch.Value)
                    {
                        table.Append(new TableRow(
                            ContentCell(Kubik(Value.Delta) + " / " + Kubik(Value.Delta / Value.Anteil) + "\t(" + Value.Typ + ")"),
                            ContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende)),
                            ContentCell(Value.Kennnummer, JustificationValues.Center),
                            ContentCell("")));
                    }
                    table.Append(new TableRow(
                        IContentCell(Kubik(Verbrauch.Value.Sum(v => v.Delta)) + " / " + Kubik(Verbrauch.Value.Sum(v => v.Delta / v.Anteil)), JustificationValues.Left, top()),
                        IContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Left, top()),
                        IContentCell(Verbrauch.Key.ToDescriptionString(), JustificationValues.Center, top()),
                        IContentCell(Percent(g.VerbrauchAnteil[Verbrauch.Key]), JustificationValues.Center, top())));
                }
            }
            return table;
        }

        private static Table ErmittlungKalteKosten(Betriebskostenabrechnung b, Rechnungsgruppe g, bool direkt = false)
        {
            var table = new Table(
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color = "888888" })),
                new TableRow(
                    ContentHead("1600", "Kostenanteil", JustificationValues.Center),
                    ContentHead("450", "Schlüssel"),
                    ContentHead("1120", "Nutzungsintervall", JustificationValues.Center),
                    ContentHead("650", "Betrag", JustificationValues.Center),
                    ContentHead("550", "Ihr Anteil", JustificationValues.Right),
                    ContentHead("630", "Ihre Kosten", JustificationValues.Right)));

            TableRow kostenPunkt(Betriebskostenrechnung rechnung, string zeitraum, int Jahr, double anteil, bool f = true)
            {
                return new TableRow(
                    ContentCell(f ? rechnung.Typ.ToDescriptionString() : ""),
                    ContentCell(direkt ? "Direkt" : (f ? rechnung.Schluessel.ToDescriptionString() : "")),
                    ContentCell(zeitraum, JustificationValues.Center),
                    ContentCell(Euro(rechnung.Betrag), JustificationValues.Right), // TODO f ? bold : normal?
                    ContentCell(Percent(anteil), JustificationValues.Right),
                    ContentCell(Euro(rechnung.Betrag * anteil), JustificationValues.Right));
            }

            foreach (var rechnung in g.Rechnungen.Where(r => (int)r.Typ % 2 == 0)) // Kalte Betriebskosten are equal / warme are odd
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
                    case UmlageSchluessel.NachVerbrauch:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        table.Append(kostenPunkt(rechnung, zeitraum, b.Jahr, g.VerbrauchAnteil[rechnung.Typ]));
                        break;
                    default:
                        break; // TODO or throw something...
                }
            }

            table.Append(new TableRow(
                ContentCell(""), ContentCell(""),
                ContentCell(""), ContentCell(""),
                ContentHead("Summe: ", JustificationValues.Center),
                ContentHead(Euro(g.Betrag), JustificationValues.Right)));
             
            return table;
        }

        private static Table ErmittlungWarmeKosten(Betriebskostenabrechnung b, Rechnungsgruppe g, bool direkt = false)
        {
            var table = new Table(new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct });
            var result = 0.0;
            foreach(var rechnung in g.Rechnungen.Where(r => (int)r.Typ % 2 == 1)) // Kalte Betriebskosten are equal / warme are odd
            {
                result += rechnung.Betrag * 1.05;
                table.Append(
                    new TableRow(
                        ContentHead("2500", rechnung.Typ.ToDescriptionString()),
                        ContentHead("2500", "Betrag")),
                    new TableRow(
                        ContentCell("Kosten für Brennstoffe"),
                        ContentCell(Euro(rechnung.Betrag))),
                    new TableRow(
                        ContentCell("Betriebskosten der Anlage (5% pauschal)"),
                        ContentCell(Euro(rechnung.Betrag * 0.05))));
            }
            table.Append(new TableRow(ContentHead("Gesamt"), ContentHead(Euro(result))));

            return table;
        }

        private static Table GesamtErgebnis(Betriebskostenabrechnung b)
        {
            var table = new Table(
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color = "888888" })),
                new TableWidth() { Width = "2500", Type = TableWidthUnitValues.Pct },
                new TableRow(
                    ContentCell("Sie haben gezahlt:"),
                    ContentCell(Euro(b.Gezahlt), JustificationValues.Right)),
                new TableRow(
                    ContentCell("Abzüglich Ihrer Kaltmiete:"),
                    ContentCell("-" + Euro(b.KaltMiete), JustificationValues.Right)));

            if (b.Minderung > 0)
            {
                table.Append(new TableRow(
                    ContentCell("Verrechnung mit Mietminderung:"),
                    ContentCell("+" + Euro(b.KaltMinderung), JustificationValues.Right)));
            }

            var f = true;
            foreach (var gruppe in b.Gruppen)
            {
                table.Append(new TableRow(
                    ContentCell(f ? "Abzüglich Ihrer Nebenkostenanteile:" : ""),
                    ContentCell("-" + Euro(gruppe.Betrag), JustificationValues.Right)));
                f = false;
            }

            if (b.Minderung > 0)
            {
                table.Append(new TableRow(
                    ContentCell("Verrechnung mit Mietminderung:"),
                    ContentCell("+" + Euro(b.NebenkostenMinderung), JustificationValues.Right)));
            }

            table.Append(new TableRow(
                ContentCell("Ergebnis:"),
                ContentHead(Euro(b.Result), JustificationValues.Right)));

            return table;
        }

        // Helper
        private static string Percent(double d) => string.Format("{0:N2}%", d * 100);
        private static string Euro(double d) => string.Format("{0:N2}€", d);
        private static string Kubik(double d) => string.Format("{0:N2} m³", d); // TODO this should be embedded in Zaehler...
        private static string Quadrat(double d) => string.Format("{0:N2} m²", d); // TODO this should be embedded in Zaehler...
        private static string Datum(DateTime d) => d.ToString("dd.MM.yyyy");

        static RunProperties Font()
        {
            var font = "Times New Roman";
            return new RunProperties(
                new RunFonts() { Ascii = font, HighAnsi = font, ComplexScript = font, },
                new FontSize() { Val = "22" }); // Size = 11
        }
        static RunProperties Bold() => new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
        static ParagraphProperties NoSpace() => new ParagraphProperties(new SpacingBetweenLines() { After = "0" });
        static Paragraph SubHeading(string str) => new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text(str)));
        static Paragraph SubHeading(string str, bool _) => new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Break(), new Text(str)));
        static Paragraph Heading(string str)
            => new Paragraph(Font(), new Run(Font(), new RunProperties(
                new Bold() { Val = OnOffValue.FromBoolean(true) },
                new Italic() { Val = OnOffValue.FromBoolean(true) }),
                new Text(str)));

        static TableCell ContentCell(string str) => new TableCell(new Paragraph(Font(), NoSpace(), new Run(Font(), new Text(str))));
        static TableCell ContentCell(string str, JustificationValues value)
            => new TableCell(
                new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(Font(), new Text(str))));

        static TableCell ContentHead(string str) => new TableCell(new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text(str))));
        static TableCell ContentHead(string pct, string str)
            => new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text(str))));
        static TableCell ContentHead(string str, JustificationValues value)
            => new TableCell(
                new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(Font(), Bold(), new Text(str))));
        static TableCell ContentHead(string pct, string str, JustificationValues value)
            => new TableCell(
                new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = pct },
                new Paragraph(Font(), NoSpace(), new ParagraphProperties(new Justification() { Val = value }),
                new Run(Font(), Bold(), new Text(str))));

        static TableCell ContentCellEnd(string str) => new TableCell(new Paragraph(Font(), new Run(Font(), new Text(str))));
    }
}
