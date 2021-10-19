using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print
{
    public interface IPrint<T>
    {
        public T body { get; }

        public void Table(int[] widths, int[] justification, string[][] cols);
        public void Introtext(Betriebskostenabrechnung b);
        public void PageBreak();
        public void Break();
        public void Text(string s);
        public void EqHeizkostenV9_2(Rechnungsgruppe gruppe);

        public void Explanation(IEnumerable<Tuple<string, string>> t);

        public void Heading(string str);
        public void SubHeading(string str);
    }

    public sealed class WordPrint : IPrint<Body>
    {
        public Body body { get; } = new Body(
            new SectionProperties(
            // Margins after DIN5008
            new PageMargin() { Left = 1418, Right = 567, Top = 958, Bottom = 958, },
            // DIN A4
            new PageSize() { Code = 9, Width = 11906, Height = 16838 }));

        public void EqHeizkostenV9_2(Rechnungsgruppe gruppe)
        {
            RunProperties rp() => new RunProperties(new RunFonts() { Ascii = "Cambria Math", HighAnsi = "Cambria Math" });
            DocumentFormat.OpenXml.Math.Run t(string str) => new DocumentFormat.OpenXml.Math.Run(rp(), new DocumentFormat.OpenXml.Math.Text(str));
            Run r(string str) => new Run(Font(), new Text(str) { Space = SpaceProcessingModeValues.Preserve });

            DocumentFormat.OpenXml.Math.OfficeMath om(string str) => new DocumentFormat.OpenXml.Math.OfficeMath(t(str));
            DocumentFormat.OpenXml.Math.OfficeMath omtw() => new DocumentFormat.OpenXml.Math.OfficeMath(tw());

            DocumentFormat.OpenXml.Math.ParagraphProperties justifyLeft
                () => new DocumentFormat.OpenXml.Math.ParagraphProperties(
                    new DocumentFormat.OpenXml.Math.Justification()
                    { Val = DocumentFormat.OpenXml.Math.JustificationValues.Left });

            DocumentFormat.OpenXml.Math.Base tw()
                => new DocumentFormat.OpenXml.Math.Base(
                        new DocumentFormat.OpenXml.Math.Subscript(
                        new DocumentFormat.OpenXml.Math.SubscriptProperties(
                        new DocumentFormat.OpenXml.Math.ControlProperties(rp())),
                        new DocumentFormat.OpenXml.Math.Base(t("t")),
                        new DocumentFormat.OpenXml.Math.SubArgument(t("w"))));

            DocumentFormat.OpenXml.Math.Fraction frac(string num, string den)
                => new DocumentFormat.OpenXml.Math.Fraction(new DocumentFormat.OpenXml.Math.FractionProperties(
                    new DocumentFormat.OpenXml.Math.ControlProperties(rp())),
                    new DocumentFormat.OpenXml.Math.Numerator(t(num)),
                    new DocumentFormat.OpenXml.Math.Denominator(t(den)));

            DocumentFormat.OpenXml.Math.Fraction units() => frac("kWh", "m³ x K");

            var p = new Paragraph(Font(), new Run(Font(), new Break(), new Text("Davon der Warmwasseranteil nach HeizkostenV §9(2):"), new Break(), new Break()));

            foreach (var hk in gruppe.Heizkosten)
            {
                p.Append(new DocumentFormat.OpenXml.Math.Paragraph(justifyLeft(),
                    new DocumentFormat.OpenXml.Math.OfficeMath(
                    t("2,5 ×"), frac("V", "Q"), t(" × ("), tw(), t("-10°C)"), units(), t(" ⟹ "),
                    t("2,5 ×"), frac(Unit(hk.V, "m³"), Unit(hk.Q, "kWh")), t(" × ("), t(Celsius((int)hk.tw)), t("-10°C)"), units(), t(" = "), t(Prozent(hk.Para9_2)))));
            }
            p.Append(new Break(), new Break(),
                new Run(Font(), r("Wobei "), om("V"), r(" die Menge des Warmwassers, die im Abrechnungszeitraum gemessen wurde, "),
                om("Q"), r(" die gemessene Wärmemenge am Allgemeinzähler und "), omtw(), r("die geschätzte mittlere Temperatur des Warmwassers darstellt.")));

            body.Append(p);
        }

        public void Table(int[] widths, int[] justification, string[][] cols)
        {
            if (widths.Count() != cols.Count() || widths.Count() != justification.Count())
            {
                throw new Exception("Table parameters are not properly specified");
            }

            var j = justification.Select(w => w == 0 ?
                JustificationValues.Left : w == 1 ?
                JustificationValues.Center :
                JustificationValues.Right).ToList();

            var table = new Table(new TableWidth() { Width = (widths.Sum() * 50).ToString(), Type = TableWidthUnitValues.Pct });
            var headrow = new TableRow();

            var heads = cols.Select(w => w.First()).ToList();
            for (var i = 0; i < widths.Count(); ++i)
            {
                headrow.Append(ContentHead((widths[i] * 50).ToString(), heads[i] ?? "", j[i]));
            }
            table.Append(headrow);

            var max = cols.Max(w => w.Length);

            for (var i = 1; i < max; ++i)
            {
                var cellrow = new TableRow();
                var row = cols.Select(w => w.Skip(i).FirstOrDefault()).ToList();
                for (var k = 0; k < row.Count(); ++k)
                {
                    cellrow.Append(ContentCell(row[k], j[k]));
                }
                table.Append(cellrow);
            }
            body.Append(table);
        }

        public void Explanation(IEnumerable<Tuple<string, string>> t)
        {
            var para = new Paragraph();

            foreach (var i in t)
            {
                para.Append(
                    new Run(
                        new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                        new Text(i.Item1 + ": ") { Space = SpaceProcessingModeValues.Preserve }),
                    new Run(new Text(i.Item2), new Break()));
            }

            body.Append(para);
        }

        public void Text(string s)
        {
            body.Append(new Paragraph(Font(), new Run(Font(), new Text(s))));
        }
        public void Introtext(Betriebskostenabrechnung b)
        {
            var p1 = new Paragraph(Font(),
                new Run(Font(),
                    new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }),
                    new Text(b.Title()),
                    new Break()),
                new Run(Font(),
                    new Text(b.Mieterliste()),
                    new Break(),
                    new Text(b.Mietobjekt()),
                    new Break(),
                    new Text("Abrechnungszeitraum: "),
                    new TabChar(),
                    new Text(b.Abrechnungszeitraum()),
                    new Break(),
                    new Text("Nutzungszeitraum: "),
                    new TabChar(),
                    new Text(b.Nutzungszeitraum())));

            var p2 = new Paragraph(Font(),
                new Run(Font(),
                new Text(b.Gruss()),
                new Break(),
                new Text(b.ResultTxt()),
                new TabChar()),
                new Run(Font(),
                new RunProperties(
                new Bold() { Val = OnOffValue.FromBoolean(true), },
                new Underline() { Val = UnderlineValues.Single, }),
                new Text(Euro(Math.Abs(b.Result))),
                new Break()),
                new Run(Font(), new Text(b.RefundDemand())));

            var p3 = new Paragraph(Font(),
                new ParagraphProperties(new Justification() { Val = JustificationValues.Both, }),
                new Run(Font(),
                new Text(b.GenerischerText())),
                new Break() { Type = BreakValues.Page });

            body.Append(p1);
            body.Append(p2);

            var Anpassung = -b.Result / 12;

            if (Anpassung > 0)
            {
                // TODO this is missing in viewmodel => move...
                body.Append(new Paragraph(
                    new Run(Font(),
                    new Text("Wir empfehlen Ihnen die monatliche Mietzahlung, um einen Betrag von " +
                    Euro(Anpassung) + " auf " + Euro(b.Gezahlt / 12 + Anpassung) + " anzupassen."))));
            }

            body.Append(p3);
        }
        public void PageBreak()
        {
            body.Append(new Break() { Type = BreakValues.Page });
        }

        public void Break()
        {
            body.Append(new Paragraph(NoSpace()));
        }


        public void Heading(string str)
        {
            var para = new Paragraph(Font(), new Run(Font(), new RunProperties(
                new Bold() { Val = OnOffValue.FromBoolean(true) },
                new Italic() { Val = OnOffValue.FromBoolean(true) }),
                new Text(str)));

            body.Append(para);
        }

        public void SubHeading(string str)
        {
            var para = new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text(str)));
            body.Append(para);
        }
    }

    public static class TPrint<T>
    {
        private static void Header(Betriebskostenabrechnung b, IPrint<T> p)
        {
            var AnsprechpartnerBezeichnung = b.Ansprechpartner is NatuerlichePerson a ? a.Vorname + " " + a.Nachname : ""; // TODO jur. Person

            var ap = b.Ansprechpartner;

            var left = new List<string> { };
            var right = new List<string> { };

            var rows = 3; // 3 Are guaranteed

            left.Add(b.Vermieter.Bezeichnung);
            if (b.Vermieter.Bezeichnung != AnsprechpartnerBezeichnung)
            {
                left.Add("℅ " + AnsprechpartnerBezeichnung);
                rows++;
            }
            left.Add(b.Ansprechpartner.Adresse!.Strasse + " " + b.Ansprechpartner.Adresse.Hausnummer);
            left.Add(b.Ansprechpartner.Adresse!.Postleitzahl + " " + b.Ansprechpartner.Adresse.Stadt);

            for (; rows < 7; rows++) { left.Add(""); }

            foreach (var m in b.Mieter)
            {
                left.Add(m.GetBriefAnrede());
                rows++;
            }
            var ad = b.Mieter.First().Adresse;
            if (ad != null)
            {
                left.Add(ad.Strasse + " " + ad.Hausnummer);
                left.Add(ad.Postleitzahl + " " + ad.Stadt);
                rows += 2;
            }

            for (; rows < 16; rows++) { left.Add(""); }

            rows = 1;

            right.Add("");
            if (ap.Telefon != null && ap.Telefon != "")
            {
                right.Add("Tel.: " + ap.Telefon);
                rows++;
            }
            if (ap.Fax != null && ap.Fax != "")
            {
                right.Add("Fax: " + ap.Fax);
                rows++;
            }
            if (ap.Email != null && ap.Email != "")
            {
                right.Add("E-Mail: " + ap.Email);
                rows++;
            }

            for (; rows < 14; rows++) { right.Add(""); }
            right.Add(DateTime.Today.ToString("dd.MM.yyyy"));

            p.Table(new int[] { 50, 50 }, new int[] { 0, 2 }, new string[][] { left.ToArray(), right.ToArray() });
        }
        private static void ExplainUmlageSchluessel(Betriebskostenabrechnung b, IPrint<T> p)
        {
            var left1 = new List<string> { };
            var right1 = new List<string> { };
            var left2 = new List<string> { };
            var right2 = new List<string> { };

            left1.Add("Umlageschlüssel");
            right1.Add("Bedeutung");
            left2.Add("Bedeutung");
            right2.Add("Beschreibung");

            if (b.dir())
            {
                left1.Add("Direkt");
                left2.Add("Direkt");
                right1.Add("Direkte Zuordnung");
                right2.Add("Kostenanteil = Kosten werden Einheit direkt zugeordnet.");
            }

            if (b.nWF())
            {
                left1.Add("n. WF.");
                left2.Add("n. WF.");
                right1.Add("nach Wohnfläche in m²");
                right2.Add("Kostenanteil = Kosten je Quadratmeter Wohnfläche mal Anteil Fläche je Wohnung.");
            }

            // There is a Umlage nach Nutzfläche in the Heizkostenberechnung:
            if (b.nNF() || b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => (int)r.Typ % 2 == 1)))
            {
                left1.Add("n. NF");
                left2.Add("n. NF");
                right1.Add("nach Nutzfläche in m²");
                right2.Add("Kostenanteil = Kosten je Quadratmeter Nutzfläche mal Anteil Fläche je Wohnung.");
            }

            if (b.nNE())
            {
                left1.Add("n. NE");
                left2.Add("n. NE");
                right1.Add("nach Anzahl der Wohn-/Nutzeinheiten");
                right2.Add("Kostenanteil = Kosten je Wohn-/Nutzeinheit.");
            }

            if (b.nPZ())
            {
                left1.Add("n. Pers.");
                left2.Add("n. Pers.");
                right1.Add("nach Personenzahl/Anzahl der Bewohner");
                right2.Add("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung.");
            }

            if (b.nVb())
            {
                left1.Add("n. Verb");
                left2.Add("n. Verb");
                right1.Add("nach Verbrauch (in m³ oder in kWh");
                right2.Add("Kostenanteil = Kosten je Verbrauchseinheit mal individuelle Verbrauchsmenge in Kubikmetern oder Kilowattstunden.");
            }

            left1.Add("");
            right1.Add("");

            var widths = new int[] { 25, 75 };
            var j = new int[] { 0, 0 };

            p.Table(widths, j, new string[][] { left1.ToArray(), right1.ToArray() });
            p.Table(widths, j, new string[][] { left2.ToArray(), right2.ToArray() });
        }

        private static void ExplainKalteBetriebskosten(Betriebskostenabrechnung b, IPrint<T> p)
        {
            var a = b.Gruppen
                .SelectMany(g => g.Rechnungen.Where(r => r.Beschreibung != null && r.Beschreibung.Trim() != "")
                .Select(t => new Tuple<string, string>(t.Typ.ToDescriptionString(), t.Beschreibung!)));
            p.Explanation(a);
        }

        private static void AbrechnungWohnung(Betriebskostenabrechnung b, Rechnungsgruppe g, IPrint<T> p)
        {
            p.SubHeading("Angaben zu Ihrer Einheit:");

            var widths = new int[] { 14, 21, 19, 12, 28, 6 };
            var col1 = new List<string> { "Nutzeinheiten" };
            var col2 = new List<string> { "Wohnfläche" };
            var col3 = new List<string> { "Nutzfläche" };
            var col4 = new List<string> { "Bewohner" };
            var col5 = new List<string> { "Nutzungsintervall" };
            var col6 = new List<string> { "Tage" };

            for (var i = 0; i < g.PersonenIntervall.Count(); ++i)
            {
                var z = g.PersonenIntervall[i];
                var f = z.Beginn.Date == b.Nutzungsbeginn.Date;

                col1.Add(f ? 1.ToString() : "");
                col2.Add(f ? Quadrat(b.Wohnung.Wohnflaeche) : "");
                col3.Add(f ? Quadrat(b.Wohnung.Nutzflaeche) : "");
                col4.Add(z.Personenzahl.ToString());
                col5.Add(Datum(z.Beginn) + " - " + Datum(z.Ende));
                col6.Add(b.Nutzungszeitspanne + "/" + b.Abrechnungszeitspanne);
            }

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }.Select(w => w.ToArray()).ToArray();

            p.Table(widths, Enumerable.Repeat(1, widths.Count()).ToArray(), cols);
        }

        private static void AbrechnungEinheit(Betriebskostenabrechnung b, Rechnungsgruppe g, IPrint<T> p)
        {
            p.SubHeading("Angaben zur Abrechnungseinheit:");
            p.Text(g.Bezeichnung);

            var widths = new int[] { 14, 21, 19, 12, 28, 6 };
            var col1 = new List<string> { "Nutzeinheiten" };
            var col2 = new List<string> { "Wohnfläche" };
            var col3 = new List<string> { "Nutzfläche" };
            var col4 = new List<string> { "Bewohner" };
            var col5 = new List<string> { "Nutzungsintervall" };
            var col6 = new List<string> { "Tage" };

            for (var i = 0; i < g.GesamtPersonenIntervall.Count(); ++i)
            {
                var z = g.GesamtPersonenIntervall[i];
                var f = z.Beginn.Date == b.Nutzungsbeginn.Date;

                var timespan = ((z.Ende - z.Beginn).Days + 1).ToString();

                col1.Add(f ? 1.ToString() : "");
                col2.Add(f ? Quadrat(g.GesamtWohnflaeche) : "");
                col3.Add(f ? Quadrat(g.GesamtNutzflaeche) : "");
                col4.Add(z.Personenzahl.ToString());
                col5.Add(Datum(z.Beginn) + " - " + Datum(z.Ende));
                col6.Add(timespan + "/" + b.Abrechnungszeitspanne);
            }

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }
                .Select(w => w.ToArray()).ToArray();

            p.Table(widths, Enumerable.Repeat(1, widths.Count()).ToArray(), cols);
        }

        private static void ErmittlungKalteEinheiten(Betriebskostenabrechnung b, Rechnungsgruppe g, IPrint<T> p)
        {
            var widths = new int[] { 39, 24, 24, 13 };
            var col1 = new List<string> { "Ermittlung Ihrer Einheiten" };
            var col2 = new List<string> { "Nutzungsintervall" };
            var col3 = new List<string> { "Tage" };
            var col4 = new List<string> { "Ihr Anteil" };

            if (g.GesamtEinheiten == 1)
            {
                col1.Add("Direkte Zuordnung");
                col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
                col4.Add(Prozent(g.WFZeitanteil));
            }

            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachWohnflaeche))
            {
                col1.Add("bei Umlage nach Wohnfläche (n. WF)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

                col1.Add(Quadrat(b.Wohnung.Wohnflaeche) + " / " + Quadrat(g.GesamtWohnflaeche));
                col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
                col4.Add(Prozent(g.WFZeitanteil));
            }

            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachNutzflaeche))
            {
                col1.Add("bei Umlage nach Nutzfläche (n. NF)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

                col1.Add(Quadrat(b.Wohnung.Nutzflaeche) + " / " + Quadrat(g.GesamtNutzflaeche));
                col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
                col4.Add(Prozent(g.NFZeitanteil));
            }

            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachNutzeinheit))
            {
                col1.Add("bei Umlage nach Nutzfläche (n. NE)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

                col1.Add(Quadrat(b.Wohnung.Nutzeinheit) + " / " + Quadrat(g.GesamtEinheiten));
                col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
                col4.Add(Prozent(g.NEZeitanteil));
            }

            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachPersonenzahl))
            {
                col1.Add("bei Umlage nach Personenzahl (n. Pers.)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

                string PersonEn(int i) => i.ToString() + (i > 1 ? " Personen" : " Person");
                for (var i = 0; i < g.PersZeitanteil.Count; ++i)
                {
                    var Beginn = g.PersZeitanteil[i].Beginn;
                    var Ende = g.PersZeitanteil[i].Ende;
                    var GesamtPersonenzahl = g.GesamtPersonenIntervall.Last(gs => gs.Beginn.Date <= g.PersZeitanteil[i].Beginn.Date).Personenzahl;
                    var Personenzahl = g.PersonenIntervall.Last(p => p.Beginn.Date <= g.PersZeitanteil[i].Beginn).Personenzahl;
                    var timespan = ((Ende - Beginn).Days + 1).ToString();

                    col1.Add(PersonEn(Personenzahl) + " / " + PersonEn(GesamtPersonenzahl));
                    col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                    col3.Add(timespan + " / " + b.Abrechnungszeitspanne.ToString());
                    col4.Add(Prozent(g.PersZeitanteil[i].Anteil));
                }
            }

            if (g.Verbrauch.Any())
            {
                col1.Add("bei Umlage nach Verbrauch (n. Verb.)");
                col2.Add("");
                col3.Add("Zählernummer");
                col4.Add("");

                foreach (var Verbrauch in g.Verbrauch.Where(v => (int)v.Key % 2 == 0)) // Kalte Betriebskosten are equal / warme are odd
                {
                    foreach (var Value in Verbrauch.Value)
                    {
                        var unit = Value.Typ.ToUnitString();
                        col1.Add(Unit(Value.Delta, unit) + " / " + Unit(Value.Delta / Value.Anteil, unit) + "\t(" + Value.Typ + ")");
                        col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                        col3.Add(Value.Kennnummer);
                        col4.Add(Verbrauch.Value.Count > 1 ? "" : Prozent(Value.Anteil));
                    }
                    if (Verbrauch.Value.Count > 1)
                    {
                        var unit = Verbrauch.Value[0].Typ.ToUnitString();
                        col1.Add(Unit(Verbrauch.Value.Sum(v => v.Delta), unit) + " / " + Unit(Verbrauch.Value.Sum(v => v.Delta / v.Anteil), unit));
                        col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                        col3.Add(Verbrauch.Key.ToDescriptionString());
                        col4.Add(Prozent(g.VerbrauchAnteil[Verbrauch.Key]));
                    }
                }
            }

            var cols = new List<List<string>> { col1, col2, col3, col4 }
                .Select(w => w.ToArray())
                .ToArray();

            p.Table(widths, new int[] { 0, 1, 1, 1 }, cols);
        }

        public static void ErmittlungKalteKosten(Betriebskostenabrechnung b, Rechnungsgruppe g, IPrint<T> p)
        {
            var widths = new int[] { 32, 9, 22, 13, 11, 13 };

            var col1 = new List<string> { "Kostenanteil" };
            var col2 = new List<string> { "Schlüssel" };
            var col3 = new List<string> { "Nutzungsintvervall" };
            var col4 = new List<string> { "Betrag" };
            var col5 = new List<string> { "Ihr Anteil" };
            var col6 = new List<string> { "Ihre Kosten" };

            void kostenPunkt(Betriebskostenrechnung rechnung, string zeitraum, int Jahr, double anteil, bool f = true)
            {
                col1.Add(f ? rechnung.Typ.ToDescriptionString() : "");
                col2.Add(g.GesamtEinheiten == 1 ? "Direkt" : (f ? rechnung.Schluessel.ToDescriptionString() : ""));
                col3.Add(zeitraum);
                col4.Add(Euro(rechnung.Betrag));
                col5.Add(Prozent(anteil));
                col6.Add(Euro(rechnung.Betrag * anteil));
            }

            foreach (var rechnung in g.Rechnungen.Where(r => (int)r.Typ % 2 == 0)) // Kalte Betriebskosten
            {
                string zeitraum;
                switch (rechnung.Schluessel)
                {
                    case UmlageSchluessel.NachWohnflaeche:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        kostenPunkt(rechnung, zeitraum, b.Jahr, g.WFZeitanteil);
                        break;
                    case UmlageSchluessel.NachNutzeinheit:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        kostenPunkt(rechnung, zeitraum, b.Jahr, g.NEZeitanteil);
                        break;
                    case UmlageSchluessel.NachPersonenzahl:
                        var first = true;
                        foreach (var a in g.PersZeitanteil)
                        {
                            zeitraum = Datum(a.Beginn) + " - " + Datum(a.Ende);
                            kostenPunkt(rechnung, zeitraum, b.Jahr, a.Anteil, first);
                            first = false;
                        }
                        break;
                    case UmlageSchluessel.NachVerbrauch:
                        zeitraum = Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende);
                        kostenPunkt(rechnung, zeitraum, b.Jahr, g.VerbrauchAnteil[rechnung.Typ]);
                        break;
                    default:
                        break; // TODO or throw something...
                }
            }

            col1.Add("");
            col2.Add("");
            col3.Add("");
            col4.Add("");
            col5.Add("Summe: ");
            col6.Add(Euro(g.BetragKalt));

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }
                .Select(w => w.ToArray())
                .ToArray();

            p.Table(widths, new int[] { 0, 0, 1, 2, 2, 2 }, cols);
        }

        private static void ErmittlungWarmeKosten(Betriebskostenabrechnung b, Rechnungsgruppe g, IPrint<T> p)
        {
            var widths = new int[] { 50, 10 };

            foreach (var rechnung in g.Rechnungen.Where(r => (int)r.Typ % 2 == 1)) // Warme Betriebskosten
            {
                var col1 = new List<string>
                {
                    rechnung.Typ.ToDescriptionString(),
                    "Kosten für Brennstoffe",
                    "Betriebskosten der Anlage (5% pauschal)"
                };
                var col2 = new List<string>
                {
                    "Betrag",
                    Euro(rechnung.Betrag),
                    Euro(rechnung.Betrag * 0.05)
                };
                var cols = new List<List<string>> { col1, col2 }.Select(w => w.ToArray()).ToArray();
                p.Table(widths, new int[] { 0, 2 }, cols);
            }
        }

        private static void ErmittlungWarmeEinheiten(Betriebskostenabrechnung b, Rechnungsgruppe g, IPrint<T> p)
        {
            var widths = new int[] { 39, 24, 24, 13 };
            var col1 = new List<string> { "Ermittlung Ihrer Einheiten" };
            var col2 = new List<string> { "Nutzungsintervall" };
            var col3 = new List<string> { "Tage" };
            var col4 = new List<string> { "Ihr Anteil" };

            col1.Add("bei Umlage nach Nutzfläche (n. NF)");
            col2.Add("");
            col3.Add("");
            col4.Add("");

            col1.Add(Quadrat(b.Wohnung.Nutzflaeche) + " / " + Quadrat(g.GesamtNutzflaeche));
            col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
            col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
            col4.Add(Prozent(g.NFZeitanteil));

            var warmeRechnungen = g.Rechnungen.Where(r => (int)r.Typ % 2 == 1).ToList();

            if (warmeRechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachPersonenzahl))
            {
                col1.Add("bei Umlage nach Personenzahl (n. Pers.)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

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

                        col1.Add(PersonEn(Personenzahl) + " / " + PersonEn(GesamtPersonenzahl));
                        col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                        col3.Add(timespan + " / " + b.Abrechnungszeitspanne.ToString());
                        col4.Add(Prozent(g.PersZeitanteil[i].Anteil));
                    }
                    else
                    {
                        col1.Add(PersonEn(Personenzahl) + " / " + PersonEn(GesamtPersonenzahl));
                        col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                        col3.Add(timespan + " / " + b.Abrechnungszeitspanne.ToString());
                        col4.Add(Prozent(g.PersZeitanteil[i].Anteil));
                    }
                }
            }

            if (warmeRechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachVerbrauch))
            {
                col1.Add("bei Umlage nach Verbrauch (n. Verb.)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

                foreach (var Verbrauch in g.Verbrauch.Where(v => (int)v.Key % 2 == 1)) // Kalte Betriebskosten are equal / warme are odd
                {
                    foreach (var Value in Verbrauch.Value)
                    {
                        var unit = Value.Typ.ToUnitString();
                        col1.Add(Unit(Value.Delta, unit) + " / " + Unit(Value.Delta / Value.Anteil, unit) + "\t(" + Value.Typ + ")");
                        col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                        col3.Add(Value.Kennnummer);
                        col4.Add(Prozent(Value.Anteil));
                    }
                }
            }

            var cols = new List<List<string>> { col1, col2, col3, col4 }.Select(w => w.ToArray()).ToArray();
            p.Table(widths, new int[] { 0, 1, 1, 1 } ,cols);
        }

        public static void ErmittlungWarmanteil(Betriebskostenabrechnung b, Rechnungsgruppe gruppe, IPrint<T> p)
        {
            var widths = new int[] { 24, 13, 9, 14, 14, 13, 13 };
            var col1 = new List<string> { "Kostenanteil" };
            var col2 = new List<string> { "Schlüssel" };
            var col3 = new List<string> { "Betrag" };
            var col4 = new List<string> { "Auft. §9(2)" };
            var col5 = new List<string> { "Auft. §7, 8" };
            var col6 = new List<string> { "Ihr Anteil" };
            var col7 = new List<string> { "Ihre Kosten" };

            foreach (var hk in gruppe.Heizkosten)
            {
                col1.Add("Heizung");
                col2.Add(UmlageSchluessel.NachNutzflaeche.ToDescriptionString());
                col3.Add(Euro(hk.PauschalBetrag));
                col4.Add(Prozent(1 - hk.Para9_2));
                col5.Add(Prozent(1 - hk.Para7));
                col6.Add(Prozent(hk.NFZeitanteil));
                col7.Add(Euro(hk.WaermeAnteilNF));

                col1.Add("Heizung");
                col2.Add(UmlageSchluessel.NachVerbrauch.ToDescriptionString());
                col3.Add(Euro(hk.PauschalBetrag));
                col4.Add(Prozent(1 - hk.Para9_2));
                col5.Add(Prozent(hk.Para7));
                col6.Add(Prozent(hk.HeizkostenVerbrauchAnteil));
                col7.Add(Euro(hk.WaermeAnteilVerb));

                col1.Add("Warmwasser");
                col2.Add(UmlageSchluessel.NachNutzflaeche.ToDescriptionString());
                col3.Add(Euro(hk.PauschalBetrag));
                col4.Add(Prozent(hk.Para9_2));
                col5.Add(Prozent(hk.Para8));
                col6.Add(Prozent(hk.NFZeitanteil));
                col7.Add(Euro(hk.WarmwasserAnteilNF));

                col1.Add("Warmwasser");
                col2.Add(UmlageSchluessel.NachVerbrauch.ToDescriptionString());
                col3.Add(Euro(hk.PauschalBetrag));
                col4.Add(Prozent(hk.Para9_2));
                col5.Add(Prozent(hk.Para8));
                col6.Add(Prozent(hk.WarmwasserVerbrauchAnteil));
                col7.Add(Euro(hk.WarmwasserAnteilVerb));

                col1.Add("");
                col2.Add("");
                col3.Add("");
                col4.Add("");
                col5.Add("");
                col6.Add("Summe: ");
                col7.Add(Euro(gruppe.BetragWarm));
            }

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6, col7 }.Select(w => w.ToArray()).ToArray();
            p.Table(widths, new int[] { 0, 0, 0, 1, 1, 1, 2 }, cols);
        }

        private static void GesamtErgebnis(Betriebskostenabrechnung b, IPrint<T> p)
        {
            var widths = new int[] { 40, 10 };

            var col1 = new List<string>
            {
                "Sie haben gezahlt:",
                "Abzüglich Ihrer Kaltmiete:",
            };

            var col2 = new List<string>
            {
                Euro(b.Gezahlt),
                "-" + Euro(b.KaltMiete),
            };

            if (b.Minderung > 0)
            {
                col1.Add("Verrechnung mit Mietminderung: ");
                col2.Add("+" + Euro(b.KaltMinderung));
            }

            var f = true;
            foreach (var gruppe in b.Gruppen)
            {
                if (gruppe.BetragKalt > 0)
                {
                    col1.Add(f ? "Abzüglich Ihrer Nebenkostenanteile: " : "");
                    col2.Add("-" + Euro(gruppe.BetragKalt));
                    f = false;
                }
            }

            foreach(var gruppe in b.Gruppen)
            {
                if (gruppe.BetragWarm > 0)
                {
                    col1.Add(f ? "Abzüglich Ihrer Nebenkostenanteile: " : "");
                    ContentCell("-" + Euro(gruppe.BetragWarm), JustificationValues.Right);
                }
            }

            if (b.Minderung > 0)
            {
                col1.Add("Verrechnung mit Mietminderung: ");
                col2.Add("+" + Euro(b.NebenkostenMinderung));
            }

            col1.Add("Ergebnis:");
            col2.Add(Euro(b.Result));

            var cols = new List<List<string>> { col1, col2 }.Select(w => w.ToArray()).ToArray();
            p.Table(widths, new int[] { 0, 2 }, cols);
        }

        public static T Print(Betriebskostenabrechnung b, IPrint<T> p)
        {
            Header(b, p);
            p.Introtext(b);
            p.PageBreak();

            p.Heading("Abrechnung der Nebenkosten");
            ExplainUmlageSchluessel(b, p);
            p.Text("Anmerkung:");
            p.Text(b.Anmerkung());
            p.Heading("Erläuterungen zu einzelnen Betriebskostenarten");
            ExplainKalteBetriebskosten(b, p);
            p.PageBreak();

            p.Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)");

            AbrechnungWohnung(b, b.Gruppen.FirstOrDefault(), p);
            foreach (var gruppe in b.Gruppen.Where(g => g.GesamtEinheiten == 1))
            {
                AbrechnungEinheit(b, gruppe, p);
                p.Break();
                ErmittlungKalteEinheiten(b, gruppe, p);
                p.Break();

                ErmittlungKalteKosten(b, gruppe, p);
            }
            p.Break();

            foreach (var gruppe in b.Gruppen.Where(g => g.GesamtEinheiten > 1))
            {
                AbrechnungEinheit(b, gruppe, p);
                p.Break();
                ErmittlungKalteEinheiten(b, gruppe, p);
                p.Break();

                ErmittlungKalteKosten(b, gruppe, p);
            }

            p.Break();
            p.Heading("Abrechnung der Nebenkosten (warme Betriebskosten)");

            foreach (var gruppe in b.Gruppen)
            {
                if (gruppe.GesamtBetragWarm > 0)
                {
                    if (gruppe.GesamtEinheiten == 1)
                    {
                        p.SubHeading("Direkt zugeordnet:");
                        ErmittlungWarmeKosten(b, gruppe, p);
                    }
                    else
                    {
                        AbrechnungEinheit(b, gruppe, p);
                        p.Break();
                        ErmittlungWarmeKosten(b, gruppe, p);
                        p.EqHeizkostenV9_2(gruppe);
                        ErmittlungWarmeEinheiten(b, gruppe, p);
                        p.Break();
                        p.SubHeading("Ermittlung der warmen Betriebskosten");
                        ErmittlungWarmanteil(b, gruppe, p);
                    }
                }
            }

            p.Heading("Gesamtergebnis der Abrechnung");
            GesamtErgebnis(b, p);

            return p.body;
        }
    }

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

        private static Body DinA4()
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
                    Erhaltungsaufwendung.ErhaltungsaufwendungWohnungBody(body, e);
                }
            }

            CreateWordDocument(filepath, body);
        }

        public static void SaveAsDocx(this ErhaltungsaufwendungWohnung e, string filepath)
        {
            var body = DinA4();
            if (!e.Liste.IsEmpty)
            {
                Erhaltungsaufwendung.ErhaltungsaufwendungWohnungBody(body, e);
            }

            CreateWordDocument(filepath, body);
        }

        public static void SaveAsDocx(this Betriebskostenabrechnung b, string filepath)
        {
            var body = TPrint<Body>.Print(b, new WordPrint());

            //FirstPage.FirstPage.Fill(body, b);
            //SecondPage.SecondPage.Fill(body, b);
            //ThirdPage.ThirdPage.Fill(body, b);

            CreateWordDocument(filepath, body);
        }
    }
}
