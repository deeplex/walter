using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print
{
    public static class TPrint<T>
    {
        private static void Header(IBetriebskostenabrechnung b, IPrint<T> p)
        {
            var AnsprechpartnerBezeichnung = b.Ansprechpartner is IPerson a ? a.Bezeichnung : "";

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

            var widths = new int[] { 50, 50 };
            var j = new int[] { 0, 2 };
            var bold = Enumerable.Repeat(false, 16).ToArray();
            var underlined = Enumerable.Repeat(false, 16).ToArray();

            p.Table(widths, j, bold, underlined, new string[][] { left.ToArray(), right.ToArray() });
        }
        private static void ExplainUmlageSchluessel(IBetriebskostenabrechnung b, IPrint<T> p)
        {
            var left1 = new List<string> { "Umlageschlüssel" };
            var right1 = new List<string> { "Bedeutung" };
            var left2 = new List<string> { "Umlageweg" };
            var right2 = new List<string> { "Beschreibung" };

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
            var bold = Enumerable.Repeat(false, left1.Count).ToArray();
            var underlined = Enumerable.Repeat(false, left1.Count).ToArray();

            p.Table(widths, j, bold, underlined, new string[][] { left1.ToArray(), right1.ToArray() });
            p.Table(widths, j, bold, underlined, new string[][] { left2.ToArray(), right2.ToArray() });
        }
        private static void ExplainKalteBetriebskosten(IBetriebskostenabrechnung b, IPrint<T> p)
        {
            var runs = b.Gruppen
                .SelectMany(g => g.Rechnungen.Where(r => r.Beschreibung != null && r.Beschreibung.Trim() != ""))
                .SelectMany(t => new List<PrintRun>()
                {
                    new PrintRun(t.Typ.ToDescriptionString() + ": ", true, false, true),
                    new PrintRun(t.Beschreibung ?? "")
                })
                .ToArray();

            p.Paragraph(runs);
        }
        private static void AbrechnungWohnung(IBetriebskostenabrechnung b, IRechnungsgruppe g, IPrint<T> p)
        {
            if (g == null) return;

            p.SubHeading("Angaben zu Ihrer Einheit:");

            var widths = new int[] { 14, 19, 19, 12, 28, 8 };
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
            var bold = Enumerable.Repeat(false, widths.Length).ToArray();
            bold[0] = true;
            var underlined = Enumerable.Repeat(false, widths.Length).ToArray();
            var justification = Enumerable.Repeat(1, widths.Length).ToArray();

            p.Table(widths, justification, bold, underlined, cols); ;
        }
        private static void AbrechnungEinheit(IBetriebskostenabrechnung b, IRechnungsgruppe g, IPrint<T> p)
        {
            p.SubHeading("Angaben zur Abrechnungseinheit:");
            p.Text(g.Bezeichnung);

            var widths = new int[] { 14, 19, 19, 12, 28, 8 };
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

            var justification = Enumerable.Repeat(1, widths.Length).ToArray();
            var bold = Enumerable.Repeat(false, col1.Count).ToArray();
            bold[0] = true;
            var underlined = Enumerable.Repeat(false, col1.Count).ToArray();

            p.Table(widths, justification, bold, underlined, cols);
        }
        private static void ErmittlungKalteEinheiten(IBetriebskostenabrechnung b, IRechnungsgruppe g, IPrint<T> p)
        {
            var widths = new int[] { 41, 22, 24, 13 };
            var col1 = new List<string> { "Ermittlung Ihrer Einheiten" };
            var col2 = new List<string> { "Nutzungsintervall" };
            var col3 = new List<string> { "Tage" };
            var col4 = new List<string> { "Ihr Anteil" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { false };

            if (g.GesamtEinheiten == 1)
            {
                col1.Add("Direkte Zuordnung");
                col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
                col4.Add(Prozent(g.WFZeitanteil));
                bold.Add(false);
                underlined.Add(true);
            }
            else
            {
                if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachWohnflaeche))
                {
                    col1.Add("bei Umlage nach Wohnfläche (n. WF)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(Quadrat(b.Wohnung.Wohnflaeche) + " / " + Quadrat(g.GesamtWohnflaeche));
                    col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                    col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
                    col4.Add(Prozent(g.WFZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachNutzflaeche))
                {
                    col1.Add("bei Umlage nach Nutzfläche (n. NF)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(Quadrat(b.Wohnung.Nutzflaeche) + " / " + Quadrat(g.GesamtNutzflaeche));
                    col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                    col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
                    col4.Add(Prozent(g.NFZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachNutzeinheit))
                {
                    col1.Add("bei Umlage nach Nutzfläche (n. NE)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(Quadrat(b.Wohnung.Nutzeinheit) + " / " + Quadrat(g.GesamtEinheiten));
                    col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                    col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
                    col4.Add(Prozent(g.NEZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachPersonenzahl))
                {
                    col1.Add("bei Umlage nach Personenzahl (n. Pers.)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

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
                        bold.Add(false);
                        underlined.Add(i == g.PersZeitanteil.Count - 1);
                    }
                }

                if (g.Verbrauch.Any())
                {
                    col1.Add("bei Umlage nach Verbrauch (n. Verb.)");
                    col2.Add("");
                    col3.Add("Zählernummer");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    foreach (var Verbrauch in g.Verbrauch.Where(v => (int)v.Key % 2 == 0)) // Kalte Betriebskosten are equal / warme are odd
                    {
                        for (var i = 0; i < Verbrauch.Value.Count; ++i)
                        {
                            var Value = Verbrauch.Value[i];
                            var unit = Value.Typ.ToUnitString();
                            col1.Add(Unit(Value.Delta, unit) + " / " + Unit(Value.Delta / Value.Anteil, unit) + "\t(" + Value.Typ + ")");
                            col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                            col3.Add(Value.Kennnummer);
                            col4.Add(Verbrauch.Value.Count > 1 ? "" : Prozent(Value.Anteil));
                            bold.Add(false);
                            underlined.Add(i == Verbrauch.Value.Count - 1);
                        }
                        if (Verbrauch.Value.Count > 1)
                        {
                            var unit = Verbrauch.Value[0].Typ.ToUnitString();
                            col1.Add(Unit(Verbrauch.Value.Sum(v => v.Delta), unit) + " / " + Unit(Verbrauch.Value.Sum(v => v.Delta / v.Anteil), unit));
                            col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                            col3.Add(Verbrauch.Key.ToDescriptionString());
                            col4.Add(Prozent(g.VerbrauchAnteil[Verbrauch.Key]));
                            bold.Add(false);
                            underlined.Add(true);
                        }
                    }
                }
            }


            var cols = new List<List<string>> { col1, col2, col3, col4 }
                .Select(w => w.ToArray())
                .ToArray();

            var justification = new int[] { 0, 1, 1, 1 };

            p.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        public static void ErmittlungKalteKosten(IBetriebskostenabrechnung b, IRechnungsgruppe g, IPrint<T> p)
        {
            var widths = new int[] { 32, 9, 22, 13, 11, 13 };

            var col1 = new List<string> { "Kostenanteil" };
            var col2 = new List<string> { "Schlüssel" };
            var col3 = new List<string> { "Nutzungsintervall" };
            var col4 = new List<string> { "Betrag" };
            var col5 = new List<string> { "Ihr Anteil" };
            var col6 = new List<string> { "Ihre Kosten" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { false };

            void kostenPunkt(Betriebskostenrechnung rechnung, string zeitraum, int Jahr, double anteil, bool f = true)
            {
                col1.Add(f ? rechnung.Typ.ToDescriptionString() : "");
                col2.Add(g.GesamtEinheiten == 1 ? "Direkt" : (f ? rechnung.Schluessel.ToDescriptionString() : ""));
                col3.Add(zeitraum);
                col4.Add(Euro(rechnung.Betrag));
                col5.Add(Prozent(anteil));
                col6.Add(Euro(rechnung.Betrag * anteil));
                bold.Add(false);
                underlined.Add(true);
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
                        kostenPunkt(
                            rechnung,
                            zeitraum,
                            b.Jahr,
                            g.VerbrauchAnteil.ContainsKey(rechnung.Typ) ? g.VerbrauchAnteil[rechnung.Typ] : 0);
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
            bold.Add(true);
            underlined.Add(false);

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }
                .Select(w => w.ToArray())
                .ToArray();

            var justification = new int[] { 0, 0, 1, 2, 2, 2 };

            p.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        private static void ErmittlungWarmeKosten(IBetriebskostenabrechnung b, IRechnungsgruppe g, IPrint<T> p)
        {
            var widths = new int[] { 50, 10 };

            foreach (var rechnung in g.Rechnungen.Where(r => (int)r.Typ % 2 == 1)) // Warme Betriebskosten
            {
                var col1 = new List<string>
                {
                    rechnung.Typ.ToDescriptionString(),
                    "Kosten für Brennstoffe",
                    "Betriebskosten der Anlage (5% pauschal)",
                    "Gesamt",
                };
                var col2 = new List<string>
                {
                    "Betrag",
                    Euro(rechnung.Betrag),
                    Euro(rechnung.Betrag * 0.05),
                    Euro(rechnung.Betrag * 1.05),
                };
                var cols = new List<List<string>> { col1, col2 }.Select(w => w.ToArray()).ToArray();

                var justification = new int[] { 0, 2 };
                var bold = new bool[] { true, false, false, true };
                var underlined = new bool[] { false, false, false, false };

                p.Table(widths, justification, bold, underlined, cols);
            }
        }
        private static void ErmittlungWarmeEinheiten(IBetriebskostenabrechnung b, IRechnungsgruppe g, IPrint<T> p)
        {
            var widths = new int[] { 41, 22, 24, 13 };
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

            var bold = new List<bool> { true, true, false };
            var underlined = new List<bool> { false, false, true };

            var warmeRechnungen = g.Rechnungen.Where(r => (int)r.Typ % 2 == 1).ToList();

            if (warmeRechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachPersonenzahl))
            {
                col1.Add("bei Umlage nach Personenzahl (n. Pers.)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

                bold.Add(true);
                bold.Add(false);

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
                    bold.Add(false);
                    underlined.Add(i == g.PersZeitanteil.Count - 1);
                }
            }

            if (warmeRechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachVerbrauch))
            {
                col1.Add("bei Umlage nach Verbrauch (n. Verb.)");
                col2.Add("");
                col3.Add("");
                col4.Add("");
                bold.Add(true);
                underlined.Add(false);

                foreach (var Verbrauch in g.Verbrauch.Where(v => (int)v.Key % 2 == 1)) // Kalte Betriebskosten are equal / warme are odd
                {
                    foreach (var Value in Verbrauch.Value)
                    {
                        var unit = Value.Typ.ToUnitString();
                        col1.Add(Unit(Value.Delta, unit) + " / " + Unit(Value.Delta / Value.Anteil, unit) + "\t(" + Value.Typ + ")");
                        col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
                        col3.Add(Value.Kennnummer);
                        col4.Add(Prozent(Value.Anteil));
                        bold.Add(false);
                        underlined.Add(false);
                    }
                }
            }

            var cols = new List<List<string>> { col1, col2, col3, col4 }.Select(w => w.ToArray()).ToArray();
            var justification = new int[] { 0, 1, 1, 1 };

            p.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        public static void ErmittlungWarmanteil(IBetriebskostenabrechnung b, IRechnungsgruppe gruppe, IPrint<T> p)
        {
            var widths = new int[] { 24, 13, 9, 14, 14, 13, 13 };
            var col1 = new List<string> { "Kostenanteil" };
            var col2 = new List<string> { "Schlüssel" };
            var col3 = new List<string> { "Betrag" };
            var col4 = new List<string> { "Auft. §9(2)" };
            var col5 = new List<string> { "Auft. §7, 8" };
            var col6 = new List<string> { "Ihr Anteil" };
            var col7 = new List<string> { "Ihre Kosten" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { true };

            foreach (var hk in gruppe.Heizkosten)
            {
                col1.Add("Heizung");
                col2.Add(UmlageSchluessel.NachNutzflaeche.ToDescriptionString());
                col3.Add(Euro(hk.PauschalBetrag));
                col4.Add(Prozent(1 - hk.Para9_2));
                col5.Add(Prozent(1 - hk.Para7));
                col6.Add(Prozent(hk.NFZeitanteil));
                col7.Add(Euro(hk.WaermeAnteilNF));
                bold.Add(false);
                underlined.Add(true);

                col1.Add("Heizung");
                col2.Add(UmlageSchluessel.NachVerbrauch.ToDescriptionString());
                col3.Add(Euro(hk.PauschalBetrag));
                col4.Add(Prozent(1 - hk.Para9_2));
                col5.Add(Prozent(hk.Para7));
                col6.Add(Prozent(hk.HeizkostenVerbrauchAnteil));
                col7.Add(Euro(hk.WaermeAnteilVerb));
                bold.Add(false);
                underlined.Add(true);

                col1.Add("Warmwasser");
                col2.Add(UmlageSchluessel.NachNutzflaeche.ToDescriptionString());
                col3.Add(Euro(hk.PauschalBetrag));
                col4.Add(Prozent(hk.Para9_2));
                col5.Add(Prozent(hk.Para8));
                col6.Add(Prozent(hk.NFZeitanteil));
                col7.Add(Euro(hk.WarmwasserAnteilNF));
                bold.Add(false);
                underlined.Add(true);

                col1.Add("Warmwasser");
                col2.Add(UmlageSchluessel.NachVerbrauch.ToDescriptionString());
                col3.Add(Euro(hk.PauschalBetrag));
                col4.Add(Prozent(hk.Para9_2));
                col5.Add(Prozent(hk.Para8));
                col6.Add(Prozent(hk.WarmwasserVerbrauchAnteil));
                col7.Add(Euro(hk.WarmwasserAnteilVerb));
                bold.Add(false);
                underlined.Add(true);

                col1.Add("");
                col2.Add("");
                col3.Add("");
                col4.Add("");
                col5.Add("");
                col6.Add("Summe: ");
                col7.Add(Euro(gruppe.BetragWarm));
                bold.Add(true);
                underlined.Add(false);
            }

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6, col7 }.Select(w => w.ToArray()).ToArray();
            var justification = new int[] { 0, 0, 0, 1, 1, 1, 2 };

            p.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        private static void GesamtErgebnis(IBetriebskostenabrechnung b, IPrint<T> p)
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

            foreach (var gruppe in b.Gruppen)
            {
                if (gruppe.BetragWarm > 0)
                {
                    col1.Add(f ? "Abzüglich Ihrer Nebenkostenanteile: " : "");
                    col2.Add("-" + Euro(gruppe.BetragWarm));
                    f = false;
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
            var justification = new int[] { 0, 2 };
            var bold = Enumerable.Repeat(false, col1.Count).ToArray();
            bold[bold.Length - 1] = true;
            var underlined = Enumerable.Repeat(true, col1.Count).ToArray();
            underlined[underlined.Length - 1] = false;

            p.Table(widths, justification, bold, underlined, cols);
        }

        private static void Introtext(IBetriebskostenabrechnung b, IPrint<T> p)
        {
            p.Paragraph(
                new PrintRun(b.Title(), true),
                new PrintRun(b.Mieterliste()),
                new PrintRun(b.Mietobjekt()),
                new PrintRun("Abrechnungszeitraum: ", false, false, true, true),
                new PrintRun(b.Abrechnungszeitraum()),
                new PrintRun("Nutzungszeitraum: ", false, false, true, true),
                new PrintRun(b.Nutzungszeitraum()));

            p.Paragraph(
                new PrintRun(b.Gruss()),
                new PrintRun(b.ResultTxt(), false, false, true, true),
                new PrintRun(Euro(Math.Abs(b.Result)), true, true),
                new PrintRun(b.RefundDemand()));

            p.Paragraph(new PrintRun(b.GenerischerText()));
        }

        public static T Print(IBetriebskostenabrechnung b, IPrint<T> p)
        {
            Header(b, p);
            Introtext(b, p);
            p.PageBreak();

            p.Heading("Abrechnung der Nebenkosten");
            ExplainUmlageSchluessel(b, p);
            p.Break();
            p.Text("Anmerkung:");
            p.Text(b.Anmerkung());
            p.Heading("Erläuterungen zu einzelnen Betriebskostenarten");
            ExplainKalteBetriebskosten(b, p);

            p.PageBreak();

            AbrechnungWohnung(b, b.Gruppen.FirstOrDefault(), p);

            p.Break();
            p.Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)");

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

            p.PageBreak();
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

        public static T Print(IErhaltungsaufwendungWohnung e, IPrint<T> p)
        {
            p.Heading(Anschrift(e.Wohnung.Adresse) + ", " + e.Wohnung.Bezeichnung);

            var widths = new int[] { 40, 15, 31, 13 };
            var col1 = new List<string> { "Aussteller" };
            var col2 = new List<string> { "Datum" };
            var col3 = new List<string> { "Bezeichnung" };
            var col4 = new List<string> { "Betrag" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { true };

            foreach (var a in e.Liste)
            {
                col1.Add(a.Aussteller.Bezeichnung);
                col2.Add(a.Datum.ToString("dd.MM.yyyy"));
                col3.Add(a.Bezeichnung);
                col4.Add(Euro(a.Betrag));
                bold.Add(false);
                underlined.Add(false);
            }

            col1.Add("");
            col2.Add("");
            col3.Add("Summe:");
            col4.Add(Euro(e.Summe));

            var justification = new int[] { 0, 1, 1, 2 };
            var cols = new List<List<string>> { col1, col2, col3, col4 }.Select(w => w.ToArray()).ToArray();

            p.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);

            return p.body;
        }
    }
}
