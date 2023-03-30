using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using static Deeplex.Saverwalter.Model.Utils;
using static Deeplex.Saverwalter.PrintService.Utils;

namespace Deeplex.Saverwalter.PrintService
{
    public static class TPrint<T>
    {
        private static void Header(IBetriebskostenabrechnung betriebskostenabrechnung, IPrint<T> printImpl)
        {
            var AnsprechpartnerBezeichnung = betriebskostenabrechnung.Ansprechpartner is IPerson a ? a.Bezeichnung : "";

            var ap = betriebskostenabrechnung.Ansprechpartner;

            var left = new List<string> { };
            var right = new List<string> { };

            var rows = 3; // 3 Are guaranteed

            left.Add(betriebskostenabrechnung.Vermieter.Bezeichnung);
            if (betriebskostenabrechnung.Vermieter.Bezeichnung != AnsprechpartnerBezeichnung)
            {
                left.Add("℅ " + AnsprechpartnerBezeichnung);
                rows++;
            }

            left.Add(betriebskostenabrechnung.Ansprechpartner.Adresse!.Strasse + " " + betriebskostenabrechnung.Ansprechpartner.Adresse.Hausnummer);
            left.Add(betriebskostenabrechnung.Ansprechpartner.Adresse!.Postleitzahl + " " + betriebskostenabrechnung.Ansprechpartner.Adresse.Stadt);

            for (; rows < 7; rows++) { left.Add(""); }

            foreach (var m in betriebskostenabrechnung.Mieter)
            {
                left.Add(m.GetBriefAnrede());
                rows++;
            }
            var ad = betriebskostenabrechnung.Mieter.First().Adresse;
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

            printImpl.Table(widths, j, bold, underlined, new string[][] { left.ToArray(), right.ToArray() });
        }
        private static void ExplainUmlageschluessel(IBetriebskostenabrechnung betriebskostenabrechnung, IPrint<T> printImpl)
        {
            var left1 = new List<string> { "Umlageschlüssel" };
            var right1 = new List<string> { "Bedeutung" };
            var left2 = new List<string> { "Umlageweg" };
            var right2 = new List<string> { "Beschreibung" };

            if (betriebskostenabrechnung.DirekteZuordnung())
            {
                left1.Add("Direkt");
                left2.Add("Direkt");
                right1.Add("Direkte Zuordnung");
                right2.Add("Kostenanteil = Kosten werden Einheit direkt zugeordnet.");
            }

            if (betriebskostenabrechnung.NachWohnflaeche())
            {
                left1.Add("n. WF.");
                left2.Add("n. WF.");
                right1.Add("nach Wohnfläche in m²");
                right2.Add("Kostenanteil = Kosten je Quadratmeter Wohnfläche mal Anteil Fläche je Wohnung.");
            }

            // There is a Umlage nach Nutzfläche in the Heizkostenberechnung:
            if (betriebskostenabrechnung.NachNutzflaeche() || betriebskostenabrechnung.Gruppen.Any(g => g.Umlagen.Where(r => r.Wohnungen.Count > 1).Any(r => (int)r.Typ % 2 == 1)))
            {
                left1.Add("n. NF");
                left2.Add("n. NF");
                right1.Add("nach Nutzfläche in m²");
                right2.Add("Kostenanteil = Kosten je Quadratmeter Nutzfläche mal Anteil Fläche je Wohnung.");
            }

            if (betriebskostenabrechnung.NachNutzeinheiten())
            {
                left1.Add("n. NE");
                left2.Add("n. NE");
                right1.Add("nach Anzahl der Wohn-/Nutzeinheiten");
                right2.Add("Kostenanteil = Kosten je Wohn-/Nutzeinheit.");
            }

            if (betriebskostenabrechnung.NachPersonenzahl())
            {
                left1.Add("n. Pers.");
                left2.Add("n. Pers.");
                right1.Add("nach Personenzahl/Anzahl der Bewohner");
                right2.Add("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung.");
            }

            if (betriebskostenabrechnung.NachVerbrauch())
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

            printImpl.Table(widths, j, bold, underlined, new string[][] { left1.ToArray(), right1.ToArray() });
            printImpl.Table(widths, j, bold, underlined, new string[][] { left2.ToArray(), right2.ToArray() });
        }
        private static void ExplainKalteBetriebskosten(IBetriebskostenabrechnung betriebskostenabrechnung, IPrint<T> printImpl)
        {
            var runs = betriebskostenabrechnung.Wohnung.Umlagen
                .Where(r => r.Beschreibung != null && r.Beschreibung.Trim() != "")
                .SelectMany(t => new List<PrintRun>()
                {
                    new PrintRun(t.Typ.ToDescriptionString() + ": ") { Bold = true },
                    new PrintRun(t.Beschreibung ?? "")
                })
                .ToArray();

            printImpl.Paragraph(runs);
        }
        private static void AbrechnungWohnung(
            IBetriebskostenabrechnung betriebskostenabrechnung,
            IRechnungsgruppe rechnungsgruppe,
            IPrint<T> printImpl)
        {
            printImpl.SubHeading("Angaben zu Ihrer Einheit:");

            var widths = new int[] { 14, 19, 19, 12, 28, 8 };
            var col1 = new List<string> { "Nutzeinheiten" };
            var col2 = new List<string> { "Wohnfläche" };
            var col3 = new List<string> { "Nutzfläche" };
            var col4 = new List<string> { "Bewohner" };
            var col5 = new List<string> { "Nutzungsintervall" };
            var col6 = new List<string> { "Tage" };

            for (var i = 0; i < rechnungsgruppe.PersonenIntervall.Count; ++i)
            {
                var personenZeitIntervall = rechnungsgruppe.PersonenIntervall[i];
                var firstLine = personenZeitIntervall.Beginn.Date == betriebskostenabrechnung.Nutzungsbeginn.Date;

                col1.Add(firstLine ? 1.ToString() : "");
                col2.Add(firstLine ? Quadrat(betriebskostenabrechnung.Wohnung.Wohnflaeche) : "");
                col3.Add(firstLine ? Quadrat(betriebskostenabrechnung.Wohnung.Nutzflaeche) : "");
                col4.Add(personenZeitIntervall.Personenzahl.ToString());
                col5.Add(Datum(personenZeitIntervall.Beginn) + " - " + Datum(personenZeitIntervall.Ende));
                col6.Add(betriebskostenabrechnung.Nutzungszeitspanne + "/" + betriebskostenabrechnung.Abrechnungszeitspanne);
            }

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }.Select(w => w.ToArray()).ToArray();
            var bold = Enumerable.Repeat(false, widths.Length).ToArray();
            bold[0] = true;
            var underlined = Enumerable.Repeat(false, widths.Length).ToArray();
            var justification = Enumerable.Repeat(1, widths.Length).ToArray();

            printImpl.Table(widths, justification, bold, underlined, cols); ;
        }
        private static void AbrechnungEinheit(IBetriebskostenabrechnung betriebskostenabrechnung, IRechnungsgruppe rechnungsgruppe, IPrint<T> printImpl)
        {
            printImpl.SubHeading("Angaben zur Abrechnungseinheit:");
            printImpl.Text(rechnungsgruppe.Bezeichnung);

            var widths = new int[] { 14, 19, 19, 12, 28, 8 };
            var col1 = new List<string> { "Nutzeinheiten" };
            var col2 = new List<string> { "Wohnfläche" };
            var col3 = new List<string> { "Nutzfläche" };
            var col4 = new List<string> { "Bewohner" };
            var col5 = new List<string> { "Nutzungsintervall" };
            var col6 = new List<string> { "Tage" };

            for (var i = 0; i < rechnungsgruppe.GesamtPersonenIntervall.Count; ++i)
            {
                var personenZeitIntervall = rechnungsgruppe.GesamtPersonenIntervall[i];
                var firstLine = personenZeitIntervall.Beginn.Date == betriebskostenabrechnung.Nutzungsbeginn.Date;

                var timespan = ((personenZeitIntervall.Ende - personenZeitIntervall.Beginn).Days + 1).ToString();

                col1.Add(firstLine ? rechnungsgruppe.GesamtEinheiten.ToString() : "");
                col2.Add(firstLine ? Quadrat(rechnungsgruppe.GesamtWohnflaeche) : "");
                col3.Add(firstLine ? Quadrat(rechnungsgruppe.GesamtNutzflaeche) : "");
                col4.Add(personenZeitIntervall.Personenzahl.ToString());
                col5.Add(Datum(personenZeitIntervall.Beginn) + " - " + Datum(personenZeitIntervall.Ende));
                col6.Add(timespan + "/" + betriebskostenabrechnung.Abrechnungszeitspanne);
            }

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }
                .Select(w => w.ToArray()).ToArray();

            var justification = Enumerable.Repeat(1, widths.Length).ToArray();
            var bold = Enumerable.Repeat(false, col1.Count).ToArray();
            bold[0] = true;
            var underlined = Enumerable.Repeat(false, col1.Count).ToArray();

            printImpl.Table(widths, justification, bold, underlined, cols);
        }
        private static void ErmittlungKalteEinheiten(
            IBetriebskostenabrechnung betriebskostenabrechnung,
            IRechnungsgruppe rechnungsgruppe,
            IPrint<T> printImpl)
        {
            var widths = new int[] { 41, 22, 24, 13 };
            var col1 = new List<string> { "Ermittlung Ihrer Einheiten" };
            var col2 = new List<string> { "Nutzungsintervall" };
            var col3 = new List<string> { "Tage" };
            var col4 = new List<string> { "Ihr Anteil" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { false };

            if (rechnungsgruppe.GesamtEinheiten == 1)
            {
                col1.Add("Direkte Zuordnung");
                col2.Add(Datum(betriebskostenabrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenabrechnung.Nutzungsende));
                col3.Add(betriebskostenabrechnung.Nutzungszeitspanne.ToString() + " / " + betriebskostenabrechnung.Abrechnungszeitspanne.ToString());
                col4.Add(Prozent(rechnungsgruppe.WFZeitanteil));
                bold.Add(false);
                underlined.Add(true);
            }
            else
            {
                if (rechnungsgruppe.Umlagen.Exists(umlage => umlage.Schluessel == Umlageschluessel.NachWohnflaeche))
                {
                    col1.Add("bei Umlage nach Wohnfläche (n. WF)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(Quadrat(betriebskostenabrechnung.Wohnung.Wohnflaeche) + " / " + Quadrat(rechnungsgruppe.GesamtWohnflaeche));
                    col2.Add(Datum(betriebskostenabrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenabrechnung.Nutzungsende));
                    col3.Add(betriebskostenabrechnung.Nutzungszeitspanne.ToString() + " / " + betriebskostenabrechnung.Abrechnungszeitspanne.ToString());
                    col4.Add(Prozent(rechnungsgruppe.WFZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (rechnungsgruppe.Umlagen.Exists(umlage => umlage.Schluessel == Umlageschluessel.NachNutzflaeche))
                {
                    col1.Add("bei Umlage nach Nutzfläche (n. NF)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(Quadrat(betriebskostenabrechnung.Wohnung.Nutzflaeche) + " / " + Quadrat(rechnungsgruppe.GesamtNutzflaeche));
                    col2.Add(Datum(betriebskostenabrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenabrechnung.Nutzungsende));
                    col3.Add(betriebskostenabrechnung.Nutzungszeitspanne.ToString() + " / " + betriebskostenabrechnung.Abrechnungszeitspanne.ToString());
                    col4.Add(Prozent(rechnungsgruppe.NFZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (rechnungsgruppe.Umlagen.Exists(umlage => umlage.Schluessel == Umlageschluessel.NachNutzeinheit))
                {
                    col1.Add("bei Umlage nach Nutzeinheiten (n. NE)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(Quadrat(betriebskostenabrechnung.Wohnung.Nutzeinheit) + " / " + rechnungsgruppe.GesamtEinheiten);
                    col2.Add(Datum(betriebskostenabrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenabrechnung.Nutzungsende));
                    col3.Add(betriebskostenabrechnung.Nutzungszeitspanne.ToString() + " / " + betriebskostenabrechnung.Abrechnungszeitspanne.ToString());
                    col4.Add(Prozent(rechnungsgruppe.NEZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (rechnungsgruppe.Umlagen.Exists(umlage => umlage.Schluessel == Umlageschluessel.NachPersonenzahl))
                {
                    col1.Add("bei Umlage nach Personenzahl (n. Pers.)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    static string SingularOrPluralPerson(int i) => i.ToString() + (i > 1 ? " Personen" : " Person");
                    for (var i = 0; i < rechnungsgruppe.PersonenZeitanteil.Count; ++i)
                    {
                        var Beginn = rechnungsgruppe.PersonenZeitanteil[i].Beginn;
                        var Ende = rechnungsgruppe.PersonenZeitanteil[i].Ende;
                        var GesamtPersonenzahl = rechnungsgruppe.GesamtPersonenIntervall.Last(gs => gs.Beginn.Date <= rechnungsgruppe.PersonenZeitanteil[i].Beginn.Date).Personenzahl;
                        var Personenzahl = rechnungsgruppe.PersonenIntervall.LastOrDefault(p => p.Beginn.Date <= rechnungsgruppe.PersonenZeitanteil[i].Beginn)?.Personenzahl ?? 0;
                        var timespan = ((Ende - Beginn).Days + 1).ToString();

                        col1.Add(SingularOrPluralPerson(Personenzahl) + " / " + SingularOrPluralPerson(GesamtPersonenzahl));
                        col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                        col3.Add(timespan + " / " + betriebskostenabrechnung.Abrechnungszeitspanne.ToString());
                        col4.Add(Prozent(rechnungsgruppe.PersonenZeitanteil[i].Anteil));
                        bold.Add(false);
                        underlined.Add(i == rechnungsgruppe.PersonenZeitanteil.Count - 1);
                    }
                }

                if (rechnungsgruppe.Umlagen.Any(e => e.Schluessel == Umlageschluessel.NachVerbrauch))
                {
                    col1.Add("bei Umlage nach Verbrauch (n. Verb.)");
                    col2.Add("");
                    col3.Add("Zählernummer");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    foreach (var Verbrauch in rechnungsgruppe.Verbrauch.Where(v => (int)v.Key % 2 == 0)) // Kalte Betriebskosten are equal / warme are odd
                    {
                        for (var i = 0; i < Verbrauch.Value.Count; ++i)
                        {
                            var Value = Verbrauch.Value[i];
                            var unit = Value.Typ.ToUnitString();
                            col1.Add(Unit(Value.Delta, unit) + " / " + Unit(Value.Delta / Value.Anteil, unit) + "\t(" + Value.Typ + ")");
                            col2.Add(Datum(betriebskostenabrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenabrechnung.Nutzungsende));
                            col3.Add(Value.Kennnummer);
                            col4.Add(Verbrauch.Value.Count > 1 ? "" : Prozent(Value.Anteil));
                            bold.Add(false);
                            underlined.Add(i == Verbrauch.Value.Count - 1);
                        }
                        if (Verbrauch.Value.Count > 1)
                        {
                            var unit = Verbrauch.Value[0].Typ.ToUnitString();
                            col1.Add(Unit(Verbrauch.Value.Sum(v => v.Delta), unit) + " / " + Unit(Verbrauch.Value.Sum(v => v.Delta / v.Anteil), unit));
                            col2.Add(Datum(betriebskostenabrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenabrechnung.Nutzungsende));
                            col3.Add(Verbrauch.Key.ToDescriptionString());
                            col4.Add(Prozent(rechnungsgruppe.VerbrauchAnteil[Verbrauch.Key]));
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

            printImpl.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        public static void ErmittlungKalteKosten(
            BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenrechnung,
            IRechnungsgruppe rechnungsgruppe,
            IPrint<T> printImpl)
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

            void kostenPunkt(Umlage umlage, string zeitraum, int Jahr, double anteil, bool firstLine = true)
            {
                var betrag = umlage.Betriebskostenrechnungen
                    .Where(rechnung => rechnung.BetreffendesJahr == betriebskostenrechnung.Jahr)
                    .Sum(b => b.Betrag);
                if (umlage.Typ == Betriebskostentyp.AllgemeinstromHausbeleuchtung)
                {
                    betrag -= betriebskostenrechnung.AllgStromFaktor;
                }
                col1.Add(firstLine ? umlage.Typ.ToDescriptionString() : "");
                col2.Add(rechnungsgruppe.GesamtEinheiten == 1 ? "Direkt" : (firstLine ? umlage.Schluessel.ToDescriptionString() : ""));
                col3.Add(zeitraum);
                col4.Add(Euro(betrag));
                col5.Add(Prozent(anteil));
                col6.Add(Euro(betrag * anteil));
                bold.Add(false);
                underlined.Add(true);
            }

            foreach (var umlage in rechnungsgruppe.Umlagen.Where(umlage => (int)umlage.Typ % 2 == 0)) // Kalte Betriebskosten
            {
                switch (umlage.Schluessel)
                {
                    case Umlageschluessel.NachWohnflaeche:
                        kostenPunkt(
                            umlage,
                            Datum(betriebskostenrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenrechnung.Nutzungsende),
                            betriebskostenrechnung.Jahr,
                            rechnungsgruppe.WFZeitanteil);
                        break;
                    case Umlageschluessel.NachNutzeinheit:
                        kostenPunkt(
                            umlage,
                            Datum(betriebskostenrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenrechnung.Nutzungsende),
                            betriebskostenrechnung.Jahr,
                            rechnungsgruppe.NEZeitanteil);
                        break;
                    case Umlageschluessel.NachPersonenzahl:
                        var first = true;
                        foreach (var a in rechnungsgruppe.PersonenZeitanteil)
                        {
                            kostenPunkt(
                                umlage,
                                Datum(a.Beginn) + " - " + Datum(a.Ende),
                                betriebskostenrechnung.Jahr,
                                a.Anteil,
                                first);
                            first = false;
                        }
                        break;
                    case Umlageschluessel.NachVerbrauch:
                        kostenPunkt(
                            umlage,
                            Datum(betriebskostenrechnung.Nutzungsbeginn) + " - " + Datum(betriebskostenrechnung.Nutzungsende),
                            betriebskostenrechnung.Jahr,
                            rechnungsgruppe.VerbrauchAnteil.ContainsKey(umlage.Typ) ? rechnungsgruppe.VerbrauchAnteil[umlage.Typ] : 0);
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
            col6.Add(Euro(rechnungsgruppe.BetragKalt));
            bold.Add(true);
            underlined.Add(false);

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }
                .Select(w => w.ToArray())
                .ToArray();

            var justification = new int[] { 0, 0, 1, 2, 2, 2 };

            printImpl.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        private static void ErmittlungWarmeKosten(BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung, IRechnungsgruppe rechnungsgruppe, IPrint<T> p)
        {
            var widths = new int[] { 50, 10 };

            foreach (var umlage in rechnungsgruppe.Umlagen.Where(umlage => (int)umlage.Typ % 2 == 1)) // Warme Betriebskosten
            {
                var betrag = umlage.Betriebskostenrechnungen
                    .Where(r => r.BetreffendesJahr == betriebskostenabrechnung.Jahr)
                    .Sum(rechnung => rechnung.Betrag);

                var col1 = new List<string>
                {
                    umlage.Typ.ToDescriptionString(),
                    "Kosten für Brennstoffe",
                    "Betriebskosten der Anlage (5% pauschal)",
                    "Gesamt",
                };
                var col2 = new List<string>
                {
                    "Betrag",
                    Euro(betrag),
                    Euro(betriebskostenabrechnung.AllgStromFaktor),
                    Euro(betrag + betriebskostenabrechnung.AllgStromFaktor),
                };
                var cols = new List<List<string>> { col1, col2 }.Select(w => w.ToArray()).ToArray();

                var justification = new int[] { 0, 2 };
                var bold = new bool[] { true, false, false, true };
                var underlined = new bool[] { false, false, false, false };

                p.Table(widths, justification, bold, underlined, cols);
            }
        }
        private static void ErmittlungWarmeEinheiten(BetriebskostenabrechnungService.IBetriebskostenabrechnung b, IRechnungsgruppe rechnungsgruppe, IPrint<T> p)
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

            col1.Add(Quadrat(b.Wohnung.Nutzflaeche) + " / " + Quadrat(rechnungsgruppe.GesamtNutzflaeche));
            col2.Add(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende));
            col3.Add(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString());
            col4.Add(Prozent(rechnungsgruppe.NFZeitanteil));

            var bold = new List<bool> { true, true, false };
            var underlined = new List<bool> { false, false, true };

            var warmeRechnungen = rechnungsgruppe.Umlagen.Where(umlage => (int)umlage.Typ % 2 == 1).ToList();

            if (warmeRechnungen.Exists(umlage => umlage.Schluessel == Umlageschluessel.NachPersonenzahl))
            {
                col1.Add("bei Umlage nach Personenzahl (n. Pers.)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

                bold.Add(true);
                bold.Add(false);

                static string SingularOrPluralPerson(int i) => i.ToString() + (i > 1 ? " Personen" : " Person");
                for (var i = 0; i < rechnungsgruppe.PersonenZeitanteil.Count; ++i)
                {
                    var Beginn = rechnungsgruppe.PersonenZeitanteil[i].Beginn;
                    var Ende = rechnungsgruppe.PersonenZeitanteil[i].Ende;
                    var GesamtPersonenzahl = rechnungsgruppe.GesamtPersonenIntervall.Last(gs => gs.Beginn.Date <= rechnungsgruppe.PersonenZeitanteil[i].Beginn.Date).Personenzahl;
                    var Personenzahl = rechnungsgruppe.PersonenIntervall.Last(p => p.Beginn.Date <= rechnungsgruppe.PersonenZeitanteil[i].Beginn).Personenzahl;
                    var timespan = ((Ende - Beginn).Days + 1).ToString();

                    if (i == rechnungsgruppe.PersonenZeitanteil.Count - 1)
                    {

                        col1.Add(SingularOrPluralPerson(Personenzahl) + " / " + SingularOrPluralPerson(GesamtPersonenzahl));
                        col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                        col3.Add(timespan + " / " + b.Abrechnungszeitspanne.ToString());
                        col4.Add(Prozent(rechnungsgruppe.PersonenZeitanteil[i].Anteil));
                    }
                    else
                    {
                        col1.Add(SingularOrPluralPerson(Personenzahl) + " / " + SingularOrPluralPerson(GesamtPersonenzahl));
                        col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                        col3.Add(timespan + " / " + b.Abrechnungszeitspanne.ToString());
                        col4.Add(Prozent(rechnungsgruppe.PersonenZeitanteil[i].Anteil));
                    }
                    bold.Add(false);
                    underlined.Add(i == rechnungsgruppe.PersonenZeitanteil.Count - 1);
                }
            }

            if (warmeRechnungen.Exists(umlage => umlage.Schluessel == Umlageschluessel.NachVerbrauch))
            {
                col1.Add("bei Umlage nach Verbrauch (n. Verb.)");
                col2.Add("");
                col3.Add("");
                col4.Add("");
                bold.Add(true);
                underlined.Add(false);

                foreach (var Verbrauch in rechnungsgruppe.Verbrauch.Where(v => (int)v.Key % 2 == 1)) // Kalte Betriebskosten are equal / warme are odd
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
        private static void ErmittlungWarmanteil(IRechnungsgruppe gruppe, IPrint<T> p)
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

            foreach (var heizkostenberechnung in gruppe.Heizkosten)
            {
                col1.Add("Heizung");
                col2.Add(Umlageschluessel.NachNutzflaeche.ToDescriptionString());
                col3.Add(Euro(heizkostenberechnung.PauschalBetrag));
                col4.Add(Prozent(1 - heizkostenberechnung.Para9_2));
                col5.Add(Prozent(1 - heizkostenberechnung.Para7));
                col6.Add(Prozent(heizkostenberechnung.NFZeitanteil));
                col7.Add(Euro(heizkostenberechnung.WaermeAnteilNF));
                bold.Add(false);
                underlined.Add(true);

                col1.Add("Heizung");
                col2.Add(Umlageschluessel.NachVerbrauch.ToDescriptionString());
                col3.Add(Euro(heizkostenberechnung.PauschalBetrag));
                col4.Add(Prozent(1 - heizkostenberechnung.Para9_2));
                col5.Add(Prozent(heizkostenberechnung.Para7));
                col6.Add(Prozent(heizkostenberechnung.HeizkostenVerbrauchAnteil));
                col7.Add(Euro(heizkostenberechnung.WaermeAnteilVerb));
                bold.Add(false);
                underlined.Add(true);

                col1.Add("Warmwasser");
                col2.Add(Umlageschluessel.NachNutzflaeche.ToDescriptionString());
                col3.Add(Euro(heizkostenberechnung.PauschalBetrag));
                col4.Add(Prozent(heizkostenberechnung.Para9_2));
                col5.Add(Prozent(heizkostenberechnung.Para8));
                col6.Add(Prozent(heizkostenberechnung.NFZeitanteil));
                col7.Add(Euro(heizkostenberechnung.WarmwasserAnteilNF));
                bold.Add(false);
                underlined.Add(true);

                col1.Add("Warmwasser");
                col2.Add(Umlageschluessel.NachVerbrauch.ToDescriptionString());
                col3.Add(Euro(heizkostenberechnung.PauschalBetrag));
                col4.Add(Prozent(heizkostenberechnung.Para9_2));
                col5.Add(Prozent(heizkostenberechnung.Para8));
                col6.Add(Prozent(heizkostenberechnung.WarmwasserVerbrauchAnteil));
                col7.Add(Euro(heizkostenberechnung.WarmwasserAnteilVerb));
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
        private static void GesamtErgebnis(BetriebskostenabrechnungService.IBetriebskostenabrechnung b, IPrint<T> p)
        {
            var widths = new int[] { 40, 10 };

            var col1 = new List<string>
            {
                "Sie haben vorausgezahlt:"
            };

            var col2 = new List<string>
            {
                Euro(b.Gezahlt - b.KaltMiete)
            };

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
            bold[^1] = true;
            var underlined = Enumerable.Repeat(true, col1.Count).ToArray();
            underlined[^1] = false;

            p.Table(widths, justification, bold, underlined, cols);
        }

        private static void Introtext(BetriebskostenabrechnungService.IBetriebskostenabrechnung b, IPrint<T> p)
        {
            p.Paragraph(
                new PrintRun(b.Title()) { Bold = true },
                new PrintRun(b.Mieterliste()),
                new PrintRun(b.Mietobjekt()),
                new PrintRun("Abrechnungszeitraum: ") { NoBreak = true, Tab = true },
                new PrintRun(b.Abrechnungszeitraum()),
                new PrintRun("Nutzungszeitraum: ") { NoBreak = true, Tab = true },
                new PrintRun(b.Nutzungszeitraum()));

            p.Paragraph(
                new PrintRun(b.Gruss()),
                new PrintRun(b.ResultTxt()) { NoBreak = true, Tab = true },
                new PrintRun(Euro(Math.Abs(b.Result))) { Bold = true, Underlined = true },
                new PrintRun(b.RefundDemand()));

            p.Paragraph(new PrintRun(b.GenerischerText()));
        }

        public static T Print(IBetriebskostenabrechnung betriebskostenabrechnung, IPrint<T> printImpl)
        {
            Header(betriebskostenabrechnung, printImpl);
            Introtext(betriebskostenabrechnung, printImpl);
            printImpl.PageBreak();

            printImpl.Heading("Abrechnung der Nebenkosten");
            ExplainUmlageschluessel(betriebskostenabrechnung, printImpl);
            printImpl.Break();
            printImpl.Text("Anmerkung:");
            printImpl.Text(betriebskostenabrechnung.Anmerkung());
            printImpl.Heading("Erläuterungen zu einzelnen Betriebskostenarten");
            ExplainKalteBetriebskosten(betriebskostenabrechnung, printImpl);

            printImpl.PageBreak();

            if (betriebskostenabrechnung.Gruppen.FirstOrDefault() is Rechnungsgruppe rechnungsgruppe)
            {
                AbrechnungWohnung(betriebskostenabrechnung, rechnungsgruppe, printImpl);
            }

            printImpl.Break();
            printImpl.Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)");

            foreach (var gruppe in betriebskostenabrechnung.Gruppen.Where(rechnungsgruppe => rechnungsgruppe.GesamtEinheiten == 1))
            {
                AbrechnungEinheit(betriebskostenabrechnung, gruppe, printImpl);
                printImpl.Break();
                ErmittlungKalteEinheiten(betriebskostenabrechnung, gruppe, printImpl);
                printImpl.Break();

                ErmittlungKalteKosten(betriebskostenabrechnung, gruppe, printImpl);
            }
            printImpl.Break();

            foreach (var gruppe in betriebskostenabrechnung.Gruppen.Where(rechnungsgruppe => rechnungsgruppe.GesamtEinheiten > 1))
            {
                AbrechnungEinheit(betriebskostenabrechnung, gruppe, printImpl);
                printImpl.Break();
                ErmittlungKalteEinheiten(betriebskostenabrechnung, gruppe, printImpl);
                printImpl.Break();

                ErmittlungKalteKosten(betriebskostenabrechnung, gruppe, printImpl);
            }

            printImpl.PageBreak();
            printImpl.Heading("Abrechnung der Nebenkosten (warme Betriebskosten)");

            foreach (var gruppe in betriebskostenabrechnung.Gruppen)
            {
                if (gruppe.GesamtBetragWarm > 0)
                {
                    if (gruppe.GesamtEinheiten == 1)
                    {
                        printImpl.SubHeading("Direkt zugeordnet:");
                        ErmittlungWarmeKosten(betriebskostenabrechnung, gruppe, printImpl);
                    }
                    else
                    {
                        AbrechnungEinheit(betriebskostenabrechnung, gruppe, printImpl);
                        printImpl.Break();
                        ErmittlungWarmeKosten(betriebskostenabrechnung, gruppe, printImpl);
                        printImpl.EqHeizkostenV9_2(gruppe);
                        ErmittlungWarmeEinheiten(betriebskostenabrechnung, gruppe, printImpl);
                        printImpl.Break();
                        printImpl.SubHeading("Ermittlung der warmen Betriebskosten");
                        ErmittlungWarmanteil(gruppe, printImpl);
                    }
                }
            }

            printImpl.Heading("Gesamtergebnis der Abrechnung");
            GesamtErgebnis(betriebskostenabrechnung, printImpl);

            return printImpl.body;
        }

        public static T Print(IErhaltungsaufwendungWohnung erhaltungsaufwendungen, IPrint<T> printImpl)
        {
            var anschrift = erhaltungsaufwendungen.Wohnung.Adresse!.Anschrift; // TODO Adresse shouldn't be null here
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
