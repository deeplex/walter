// Copyright (c) 2023-2026 Kai Lawrence
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
using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using static Deeplex.Saverwalter.PrintService.Utils;

namespace Deeplex.Saverwalter.PrintService
{
    public static class TPrint<T>
    {
        private static void Header(NkDruckdaten d, IPrint<T> p)
        {
            var left = new List<string>();
            var right = new List<string>();
            var rows = 3;

            left.Add(d.Vermieter.Bezeichnung);
            if (d.Vermieter.Bezeichnung != d.Ansprechpartner.Bezeichnung)
            {
                left.Add("℅ " + d.Ansprechpartner.Bezeichnung);
                rows++;
            }

            if (d.Ansprechpartner.Adresse == null)
                d.Notes.Add("Ansprechpartner hat keine Adresse. Dies ist notwendig.", Severity.Error);

            left.Add(d.Ansprechpartner.Adresse!.Strasse + " " + d.Ansprechpartner.Adresse.Hausnummer);
            left.Add(d.Ansprechpartner.Adresse.Postleitzahl + " " + d.Ansprechpartner.Adresse.Stadt);
            for (; rows < 7; rows++) left.Add("");

            foreach (var m in d.Mieter) { left.Add(GetBriefAnrede(m)); rows++; }

            var mieterAdresse = d.Mieter.FirstOrDefault(m => m.Adresse != null)?.Adresse;
            if (mieterAdresse != null)
            {
                left.Add(mieterAdresse.Strasse + " " + mieterAdresse.Hausnummer);
                left.Add(mieterAdresse.Postleitzahl + " " + mieterAdresse.Stadt);
            }
            else { left.Add(""); left.Add(""); }
            rows += 2;
            for (; rows < 16; rows++) left.Add("");

            rows = 1;
            right.Add("");
            if (!string.IsNullOrEmpty(d.Ansprechpartner.Telefon)) { right.Add("Tel.: " + d.Ansprechpartner.Telefon); rows++; }
            if (!string.IsNullOrEmpty(d.Ansprechpartner.Fax)) { right.Add("Fax: " + d.Ansprechpartner.Fax); rows++; }
            if (!string.IsNullOrEmpty(d.Ansprechpartner.Email)) { right.Add("E-Mail: " + d.Ansprechpartner.Email); rows++; }
            for (; rows < 14; rows++) right.Add("");
            right.Add(DateTime.Today.ToString("dd.MM.yyyy"));

            p.Table(new int[] { 50, 50 }, new int[] { 0, 2 },
                Enumerable.Repeat(false, 16).ToArray(),
                Enumerable.Repeat(false, 16).ToArray(),
                new string[][] { left.ToArray(), right.ToArray() });
        }

        private static void IntrotextNk(NkDruckdaten d, IPrint<T> p)
        {
            var abrechnungsbeginn = new DateOnly(d.Jahr, 1, 1);
            var abrechnungsende = new DateOnly(d.Jahr, 12, 31);

            p.Paragraph(
                new PrintRun(Title(d.Jahr)) { Bold = true },
                new PrintRun(Mieterliste(d.Mieter)),
                new PrintRun(Mietobjekt(d.Wohnung)),
                new PrintRun("Abrechnungszeitraum: ") { NoBreak = true, Tab = true },
                new PrintRun(Datum(abrechnungsbeginn) + " - " + Datum(abrechnungsende)),
                new PrintRun("Nutzungszeitraum: ") { NoBreak = true, Tab = true },
                new PrintRun(Datum(d.Nutzungsbeginn) + " - " + Datum(d.Nutzungsende)));

            p.Paragraph(
                new PrintRun(Gruss(d.Mieter)),
                new PrintRun(ResultTxt(d.Saldo)) { NoBreak = true, Tab = true },
                new PrintRun(Euro(Math.Abs(d.Saldo))) { Bold = true, Underlined = true },
                new PrintRun(RefundDemand(d.Saldo)));

            var hasWarm = d.Einheiten.Any(e => e.BetragWarm != 0);
            var text = GenerischerTextFirstPart
                + (hasWarm ? GenerischerTextHeizungPart : "")
                + GenerischerTextFinalPart;
            p.Paragraph(new PrintRun(text));
        }

        private static void ExplainUmlageschluesselNk(IReadOnlyList<NkDruckEinheit> einheiten, IPrint<T> p)
        {
            var left1 = new List<string> { "Umlageschlüssel" };
            var right1 = new List<string> { "Bedeutung" };
            var left2 = new List<string> { "Umlageweg" };
            var right2 = new List<string> { "Beschreibung" };

            bool HasSchluessel(Umlageschluessel s) =>
                einheiten.Any(e => !e.IstDirekt && e.Rechnungen.Any(r => r.Umlage.Schluessel == s));

            if (einheiten.Any(e => e.IstDirekt && e.Rechnungen.Count > 0))
            {
                left1.Add("Direkt"); right1.Add("Direkte Zuordnung");
                left2.Add("Direkt"); right2.Add("Kostenanteil = Kosten werden Einheit direkt zugeordnet.");
            }
            if (HasSchluessel(Umlageschluessel.NachWohnflaeche))
            {
                left1.Add("n. WF."); right1.Add("nach Wohnfläche in m²");
                left2.Add("n. WF."); right2.Add("Kostenanteil = Kosten je Quadratmeter Wohnfläche mal Anteil Fläche je Wohnung.");
            }
            if (HasSchluessel(Umlageschluessel.NachNutzflaeche))
            {
                left1.Add("n. NF"); right1.Add("nach Nutzfläche in m²");
                left2.Add("n. NF"); right2.Add("Kostenanteil = Kosten je Quadratmeter Nutzfläche mal Anteil Fläche je Wohnung.");
            }
            if (HasSchluessel(Umlageschluessel.NachMiteigentumsanteil))
            {
                left1.Add("n. MEA"); right1.Add("nach Miteigentumsanteilen");
                left2.Add("n. MEA"); right2.Add("Kostenanteil = Kosten je Miteigentumsanteil je Wohnung");
            }
            if (HasSchluessel(Umlageschluessel.NachNutzeinheit))
            {
                left1.Add("n. NE"); right1.Add("nach Anzahl der Wohn-/Nutzeinheiten");
                left2.Add("n. NE"); right2.Add("Kostenanteil = Kosten je Wohn-/Nutzeinheit.");
            }
            if (HasSchluessel(Umlageschluessel.NachPersonenzahl))
            {
                left1.Add("n. Pers."); right1.Add("nach Personenzahl/Anzahl der Bewohner");
                left2.Add("n. Pers."); right2.Add("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung.");
            }
            if (HasSchluessel(Umlageschluessel.NachVerbrauch))
            {
                left1.Add("n. Verb."); right1.Add("nach Verbrauch (in m³ oder in kWh");
                left2.Add("n. Verb."); right2.Add("Kostenanteil = Kosten je Verbrauchseinheit mal individuelle Verbrauchsmenge in Kubikmetern oder Kilowattstunden.");
            }

            left1.Add(""); right1.Add("");

            var widths = new int[] { 25, 75 };
            var j = new int[] { 0, 0 };
            var bold = Enumerable.Repeat(false, left1.Count).ToArray();
            var underlined = Enumerable.Repeat(false, left1.Count).ToArray();
            p.Table(widths, j, bold, underlined, new string[][] { left1.ToArray(), right1.ToArray() });
            p.Table(widths, j, bold, underlined, new string[][] { left2.ToArray(), right2.ToArray() });
        }

        private static void ExplainKalteBetriebskostenNk(NkDruckdaten d, IPrint<T> p)
        {
            var runs = d.Einheiten
                .SelectMany(e => e.Rechnungen)
                .Select(r => r.Umlage)
                .Where(u => !string.IsNullOrWhiteSpace(u.Beschreibung))
                .DistinctBy(u => u.UmlageId)
                .SelectMany(u => new List<PrintRun>
                {
                    new PrintRun(u.Typ.Bezeichnung + ": ") { Bold = true, NoBreak = true },
                    new PrintRun(u.Beschreibung ?? "")
                })
                .ToArray();

            if (runs.Length > 0)
                p.Paragraph(runs);
        }

        private static void ErmittlungKalteEinheitenNk(NkDruckEinheit einheit, NkDruckdaten d, IPrint<T> p)
        {
            var widths = new int[] { 41, 25, 17, 17 };
            var col1 = new List<string> { "Ermittlung Ihrer Anteile" };
            var col2 = new List<string> { "Nutzungsintervall" };
            var col3 = new List<string> { "Tage" };
            var col4 = new List<string> { "Ihr Anteil" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { false };

            var nutzungszeitraum = d.Nutzungsende.DayNumber - d.Nutzungsbeginn.DayNumber + 1;
            var nutzungsIntervall = Datum(d.Nutzungsbeginn) + " - " + Datum(d.Nutzungsende);
            var zeitraumFraction = nutzungszeitraum.ToString() + " / " + d.Abrechnungstage.ToString();

            if (einheit.IstDirekt)
            {
                col1.Add("nach direkter Zuordnung");
                col2.Add(nutzungsIntervall);
                col3.Add(zeitraumFraction);
                col4.Add(Prozent(einheit.WFZeitanteil));
                bold.Add(false); underlined.Add(true);
            }
            else
            {
                if (einheit.Rechnungen.Any(r => r.Umlage.Schluessel == Umlageschluessel.NachWohnflaeche))
                {
                    col1.Add("nach Wohnfläche (n. WF)"); col2.Add(""); col3.Add(""); col4.Add("");
                    bold.Add(true); underlined.Add(false);
                    col1.Add(Quadrat(d.Wohnung.Wohnflaeche) + " / " + Quadrat(einheit.GesamtWohnflaeche));
                    col2.Add(nutzungsIntervall); col3.Add(zeitraumFraction); col4.Add(Prozent(einheit.WFZeitanteil));
                    bold.Add(false); underlined.Add(true);
                }
                if (einheit.Rechnungen.Any(r => r.Umlage.Schluessel == Umlageschluessel.NachNutzflaeche))
                {
                    col1.Add("nach Nutzfläche (n. NF)"); col2.Add(""); col3.Add(""); col4.Add("");
                    bold.Add(true); underlined.Add(false);
                    col1.Add(Quadrat(d.Wohnung.Nutzflaeche) + " / " + Quadrat(einheit.GesamtNutzflaeche));
                    col2.Add(nutzungsIntervall); col3.Add(zeitraumFraction); col4.Add(Prozent(einheit.NFZeitanteil));
                    bold.Add(false); underlined.Add(true);
                }
                if (einheit.Rechnungen.Any(r => r.Umlage.Schluessel == Umlageschluessel.NachMiteigentumsanteil))
                {
                    col1.Add("nach Miteigentumsanteilen (n. MEA)"); col2.Add(""); col3.Add(""); col4.Add("");
                    bold.Add(true); underlined.Add(false);
                    col1.Add(d.Wohnung.Miteigentumsanteile + " / " + einheit.GesamtMiteigentumsanteile);
                    col2.Add(nutzungsIntervall); col3.Add(zeitraumFraction); col4.Add(Prozent(einheit.MEAZeitanteil));
                    bold.Add(false); underlined.Add(true);
                }
                if (einheit.Rechnungen.Any(r => r.Umlage.Schluessel == Umlageschluessel.NachNutzeinheit))
                {
                    col1.Add("nach Nutzeinheiten (n. NE)"); col2.Add(""); col3.Add(""); col4.Add("");
                    bold.Add(true); underlined.Add(false);
                    col1.Add(d.Wohnung.Nutzeinheit + " / " + einheit.GesamtNutzeinheiten);
                    col2.Add(nutzungsIntervall); col3.Add(zeitraumFraction); col4.Add(Prozent(einheit.NEZeitanteil));
                    bold.Add(false); underlined.Add(true);
                }
                if (einheit.Rechnungen.Any(r => r.Umlage.Schluessel == Umlageschluessel.NachPersonenzahl))
                {
                    col1.Add("nach Personenzahl (n. Pers.)"); col2.Add(""); col3.Add(""); col4.Add("");
                    bold.Add(true); underlined.Add(false);

                    static string PersonStr(int i) => i + (i > 1 ? " Personen" : " Person");
                    var anteile = einheit.PersonenZeitanteile.Where(a => a.Personenzahl > 0).ToList();
                    foreach (var a in anteile)
                    {
                        var tage = (a.Ende.DayNumber - a.Beginn.DayNumber + 1).ToString();
                        col1.Add(PersonStr(a.Personenzahl) + " / " + PersonStr(a.GesamtPersonenzahl));
                        col2.Add(Datum(a.Beginn) + " - " + Datum(a.Ende));
                        col3.Add(tage + " / " + d.Abrechnungstage.ToString());
                        col4.Add(Prozent(a.Anteil));
                        bold.Add(false); underlined.Add(a == anteile.Last());
                    }
                }
                if (einheit.Rechnungen.Any(r => r.Umlage.Schluessel == Umlageschluessel.NachVerbrauch))
                {
                    col1.Add("nach Verbrauch (n. Verb.)"); col2.Add(""); col3.Add("Zählernummer"); col4.Add("");
                    bold.Add(true); underlined.Add(false);

                    var verbrauchRechnungen = einheit.Rechnungen
                        .Where(r => r.Umlage.Schluessel == Umlageschluessel.NachVerbrauch && r.VerbrauchAnteil != null);
                    foreach (var rechnung in verbrauchRechnungen)
                    {
                        var va = rechnung.VerbrauchAnteil!;
                        foreach (var zaehlerTyp in va.DieseZaehler)
                        {
                            var typ = zaehlerTyp.Key;
                            var unit = typ.ToUnitString();
                            var deltaAll = va.AlleZaehler[typ].Sum(e => e.Delta);
                            var value = zaehlerTyp.Value;

                            for (var i = 0; i < value.Count; ++i)
                            {
                                var delta = value[i].Delta;
                                col1.Add(Unit(delta, unit) + " / " + Unit(deltaAll, unit) + " (" + rechnung.Umlage.Typ.Bezeichnung + ")");
                                col2.Add(""); col3.Add(value[i].Zaehler.Kennnummer);
                                col4.Add(Prozent(delta / deltaAll));
                                bold.Add(false); underlined.Add(false);
                            }
                            var deltaHere = va.DieseZaehler[typ].Sum(e => e.Delta);
                            col1.Add(Unit(deltaHere, unit) + " / " + Unit(deltaAll, unit));
                            col2.Add(nutzungsIntervall); col3.Add(zeitraumFraction);
                            col4.Add(Prozent(va.Anteil[typ]));
                            bold.Add(true); underlined.Add(true);
                        }
                    }
                }
            }

            var cols = new List<List<string>> { col1, col2, col3, col4 }.Select(w => w.ToArray()).ToArray();
            p.Table(widths, new int[] { 0, 1, 1, 1 }, bold.ToArray(), underlined.ToArray(), cols);
        }

        private static void ErmittlungKalteKostenNk(NkDruckEinheit einheit, NkDruckdaten d, IPrint<T> p)
        {
            var widths = new int[] { 32, 9, 25, 10, 11, 13 };
            var col1 = new List<string> { "Kostenanteil" };
            var col2 = new List<string> { "Schlüssel" };
            var col3 = new List<string> { "Nutzungsintervall" };
            var col4 = new List<string> { "Betrag" };
            var col5 = new List<string> { "Ihr Anteil" };
            var col6 = new List<string> { "Ihre Kosten" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { false };

            var nutzungsIntervall = Datum(d.Nutzungsbeginn) + " - " + Datum(d.Nutzungsende);

            foreach (var rechnung in einheit.Rechnungen)
            {
                var umlage = rechnung.Umlage;
                switch (umlage.Schluessel)
                {
                    case Umlageschluessel.NachWohnflaeche:
                    case Umlageschluessel.NachNutzflaeche:
                    case Umlageschluessel.NachNutzeinheit:
                    case Umlageschluessel.NachMiteigentumsanteil:
                    case Umlageschluessel.NachVerbrauch:
                        col1.Add(umlage.Typ.Bezeichnung);
                        col2.Add(einheit.IstDirekt ? "Direkt" : umlage.Schluessel.ToDescriptionString());
                        col3.Add(nutzungsIntervall);
                        col4.Add(Euro(rechnung.Gesamtbetrag));
                        col5.Add(Prozent(rechnung.AnteilFaktor));
                        col6.Add(Euro(rechnung.MeinBetrag));
                        bold.Add(false); underlined.Add(true);
                        break;
                    case Umlageschluessel.NachPersonenzahl:
                        var relevant = einheit.PersonenZeitanteile.Where(a => a.Personenzahl > 0).ToList();
                        if (relevant.Count == 0)
                        {
                            col1.Add(umlage.Typ.Bezeichnung);
                            col2.Add(umlage.Schluessel.ToDescriptionString());
                            col3.Add(nutzungsIntervall);
                            col4.Add(Euro(rechnung.Gesamtbetrag));
                            col5.Add(Prozent(0)); col6.Add(Euro(0));
                            bold.Add(false); underlined.Add(true);
                        }
                        else
                        {
                            var first = true;
                            foreach (var a in relevant)
                            {
                                col1.Add(first ? umlage.Typ.Bezeichnung : "");
                                col2.Add(first ? umlage.Schluessel.ToDescriptionString() : "");
                                col3.Add(Datum(a.Beginn) + " - " + Datum(a.Ende));
                                col4.Add(first ? Euro(rechnung.Gesamtbetrag) : "");
                                col5.Add(Prozent(a.Anteil));
                                col6.Add(Euro(rechnung.Gesamtbetrag * a.Anteil));
                                bold.Add(false); underlined.Add(a == relevant.Last());
                                first = false;
                            }
                        }
                        break;
                }
            }

            col1.Add(""); col2.Add(""); col3.Add(""); col4.Add("");
            col5.Add("Summe: "); col6.Add(Euro(einheit.BetragKalt));
            bold.Add(true); underlined.Add(false);

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }.Select(w => w.ToArray()).ToArray();
            p.Table(widths, new int[] { 0, 0, 1, 2, 2, 2 }, bold.ToArray(), underlined.ToArray(), cols);
        }

        private static void ErmittlungWarmeKostenNk(NkDruckHkvoRechnung hr, NkDruckdaten d, IPrint<T> p)
        {
            var widths = new int[] { 38, 10, 24, 11, 11, 6 };
            var col1 = new List<string> { "Kostenanteil" };
            var col2 = new List<string> { "Anteil" };
            var col3 = new List<string> { "Nutzungsintervall" };
            var col4 = new List<string> { "Betrag" };
            var col5 = new List<string> { "Ihr Anteil" };
            var col6 = new List<string> { "Ihre Kosten" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { false };

            var nutzungsIntervall = Datum(d.Nutzungsbeginn) + " - " + Datum(d.Nutzungsende);

            // §7 Heizungsanteil
            col1.Add("§7 Heizung (" + Prozent(1 - hr.P9_2.Para9_2) + " des Gesamtbetrags)");
            col2.Add(""); col3.Add(""); col4.Add(Euro(hr.HeizBetrag));
            col5.Add(""); col6.Add("");
            bold.Add(true); underlined.Add(false);

            if (hr.HeizVerbrauchAlle > 0)
            {
                var unit = "kWh";
                col1.Add(Unit(hr.HeizVerbrauchDiese, unit) + " / " + Unit(hr.HeizVerbrauchAlle, unit));
                col2.Add(Prozent(hr.P7) + " n.V."); col3.Add(nutzungsIntervall);
                col4.Add(""); col5.Add(Prozent(hr.P7 * hr.HeizVerbrauchAnteil));
                col6.Add(Euro(hr.HeizBetrag * hr.P7 * hr.HeizVerbrauchAnteil));
                bold.Add(false); underlined.Add(false);
            }

            col1.Add(Quadrat(d.Wohnung.Wohnflaeche));
            col2.Add(Prozent(1 - hr.P7) + " n.WF"); col3.Add(nutzungsIntervall);
            col4.Add(""); col5.Add(Prozent((1 - hr.P7) * hr.WFZeitanteil));
            col6.Add(Euro(hr.HeizBetrag * (1 - hr.P7) * hr.WFZeitanteil));
            bold.Add(false); underlined.Add(true);

            // §8 Warmwasseranteil
            col1.Add("§8 Warmwasser (" + Prozent(hr.P9_2.Para9_2) + " des Gesamtbetrags)");
            col2.Add(""); col3.Add(""); col4.Add(Euro(hr.WWBetrag));
            col5.Add(""); col6.Add("");
            bold.Add(true); underlined.Add(false);

            if (hr.WWVerbrauchAlle > 0)
            {
                var unit = "m³";
                col1.Add(Unit(hr.WWVerbrauchDiese, unit) + " / " + Unit(hr.WWVerbrauchAlle, unit));
                col2.Add(Prozent(hr.P8) + " n.V."); col3.Add(nutzungsIntervall);
                col4.Add(""); col5.Add(Prozent(hr.P8 * hr.WWVerbrauchAnteil));
                col6.Add(Euro(hr.WWBetrag * hr.P8 * hr.WWVerbrauchAnteil));
                bold.Add(false); underlined.Add(false);
            }

            col1.Add(Quadrat(d.Wohnung.Wohnflaeche));
            col2.Add(Prozent(1 - hr.P8) + " n.WF"); col3.Add(nutzungsIntervall);
            col4.Add(""); col5.Add(Prozent((1 - hr.P8) * hr.WFZeitanteil));
            col6.Add(Euro(hr.WWBetrag * (1 - hr.P8) * hr.WFZeitanteil));
            bold.Add(false); underlined.Add(true);

            col1.Add(""); col2.Add(""); col3.Add(""); col4.Add(Euro(hr.Gesamtbetrag));
            col5.Add("Summe: "); col6.Add(Euro(hr.MeinBetragGesamt));
            bold.Add(true); underlined.Add(false);

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }.Select(w => w.ToArray()).ToArray();
            p.Table(widths, new int[] { 0, 0, 1, 2, 2, 2 }, bold.ToArray(), underlined.ToArray(), cols);
        }

        private static void GesamtErgebnisNk(NkDruckdaten d, IPrint<T> p)
        {
            var widths = new int[] { 40, 20 };
            var col1 = new List<string> { "Sie haben vorausgezahlt:" };
            var col2 = new List<string> { Euro(d.Vorauszahlung) };

            var f = true;
            foreach (var einheit in d.Einheiten)
            {
                if (einheit.BetragKalt > 0)
                {
                    col1.Add(f ? "Abzüglich Ihrer Nebenkostenanteile: " : "");
                    col2.Add("-" + Euro(einheit.BetragKalt));
                    f = false;
                }
            }
            foreach (var einheit in d.Einheiten)
            {
                if (einheit.BetragWarm > 0)
                {
                    col1.Add(f ? "Abzüglich Ihrer Nebenkostenanteile: " : "");
                    col2.Add("-" + Euro(einheit.BetragWarm));
                    f = false;
                }
            }

            if (d.Mietminderung > 0)
            {
                col1.Add("Verrechnung mit Mietminderung: ");
                col2.Add("+" + Euro(d.NebenkostenMietminderung));
            }

            col1.Add("Ergebnis:"); col2.Add(Euro(d.Saldo));

            var cols = new List<List<string>> { col1, col2 }.Select(w => w.ToArray()).ToArray();
            var bold = Enumerable.Repeat(false, col1.Count).ToArray();
            bold[^1] = true;
            var underlined = Enumerable.Repeat(true, col1.Count).ToArray();
            underlined[^1] = false;

            p.Table(widths, new int[] { 0, 2 }, bold, underlined, cols);
        }

        public static T Print(NkDruckdaten druckdaten, IPrint<T> printImpl)
        {
            Header(druckdaten, printImpl);
            IntrotextNk(druckdaten, printImpl);
            printImpl.PageBreak();

            printImpl.Heading("Abrechnung der Nebenkosten");
            ExplainUmlageschluesselNk(druckdaten.Einheiten, printImpl);
            printImpl.Break();
            printImpl.Text("Anmerkung:");
            printImpl.Text(Anmerkung);
            printImpl.Heading("Erläuterungen zu einzelnen Betriebskostenarten");
            ExplainKalteBetriebskostenNk(druckdaten, printImpl);

            printImpl.PageBreak();

            printImpl.Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)");

            foreach (var einheit in druckdaten.Einheiten.Where(e => e.IstDirekt))
            {
                printImpl.Break();
                ErmittlungKalteEinheitenNk(einheit, druckdaten, printImpl);
                printImpl.Break();
                ErmittlungKalteKostenNk(einheit, druckdaten, printImpl);
            }
            printImpl.Break();
            foreach (var einheit in druckdaten.Einheiten.Where(e => !e.IstDirekt))
            {
                printImpl.Break();
                ErmittlungKalteEinheitenNk(einheit, druckdaten, printImpl);
                printImpl.Break();
                ErmittlungKalteKostenNk(einheit, druckdaten, printImpl);
            }

            var hkvoEinheiten = druckdaten.Einheiten.Where(e => e.HkvoRechnungen.Count > 0).ToList();
            if (hkvoEinheiten.Count > 0)
            {
                printImpl.PageBreak();
                printImpl.Heading("Abrechnung der Heizkosten (warme Betriebskosten)");
                printImpl.Text(Utils.GenerischerTextHeizungPart);
                foreach (var einheit in hkvoEinheiten)
                {
                    printImpl.Break();
                    printImpl.SubHeading(einheit.Bezeichnung);
                    foreach (var hr in einheit.HkvoRechnungen)
                    {
                        printImpl.EqHeizkostenV9_2(hr.P9_2);
                        printImpl.Break();
                        ErmittlungWarmeKostenNk(hr, druckdaten, printImpl);
                    }
                }
            }

            printImpl.Heading("Gesamtergebnis der Abrechnung");
            GesamtErgebnisNk(druckdaten, printImpl);

            return printImpl.body;
        }

        public static T Print(IErhaltungsaufwendungWohnung erhaltungsaufwendungen, IPrint<T> printImpl)
        {
            var anschrift = erhaltungsaufwendungen.Wohnung.Adresse!.Anschrift;
            printImpl.Heading($"{anschrift}, {erhaltungsaufwendungen.Wohnung.Bezeichnung}");

            var widths = new int[] { 40, 15, 31, 13 };
            var col1 = new List<string> { "Aussteller" };
            var col2 = new List<string> { "Datum" };
            var col3 = new List<string> { "Bezeichnung" };
            var col4 = new List<string> { "Betrag" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { true };

            foreach (var a in erhaltungsaufwendungen.Liste)
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
            col4.Add(Euro(erhaltungsaufwendungen.Summe));

            var justification = new int[] { 0, 1, 1, 2 };
            var cols = new List<List<string>> { col1, col2, col3, col4 }.Select(w => w.ToArray()).ToArray();

            printImpl.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);

            return printImpl.body;
        }
    }
}
