using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.Utils;
using static Deeplex.Saverwalter.PrintService.Utils;

namespace Deeplex.Saverwalter.PrintService
{
    public static class TPrint<T>
    {
        private static void Header(Betriebskostenabrechnung abrechnung, IPrint<T> printImpl)
        {
            var ansprechpartner = abrechnung.Ansprechpartner;

            var left = new List<string> { };
            var right = new List<string> { };

            var rows = 3; // 3 Are guaranteed

            left.Add(abrechnung.Vermieter.Bezeichnung);
            if (abrechnung.Vermieter.Bezeichnung != ansprechpartner.Bezeichnung)
            {
                left.Add("℅ " + ansprechpartner.Bezeichnung);
                rows++;
            }

            if (ansprechpartner.Adresse == null)
            {
                abrechnung.Notes.Add("Ansprechpartner hat keine Adresse. Dies ist notwendig.", Severity.Error);
            }

            left.Add(ansprechpartner.Adresse!.Strasse + " " + ansprechpartner.Adresse.Hausnummer);
            left.Add(ansprechpartner.Adresse.Postleitzahl + " " + ansprechpartner.Adresse.Stadt);

            for (; rows < 7; rows++) { left.Add(""); }

            foreach (var m in abrechnung.Mieter)
            {
                left.Add(GetBriefAnrede(m));
                rows++;
            }
            var mieterAdresse = abrechnung.Mieter.First(mieter => mieter.Adresse != null).Adresse;
            if (mieterAdresse != null)
            {
                left.Add(mieterAdresse.Strasse + " " + mieterAdresse.Hausnummer);
                left.Add(mieterAdresse.Postleitzahl + " " + mieterAdresse.Stadt);
                rows += 2;
            }

            for (; rows < 16; rows++) { left.Add(""); }

            rows = 1;

            right.Add("");
            if (ansprechpartner.Telefon != null && ansprechpartner.Telefon != "")
            {
                right.Add("Tel.: " + ansprechpartner.Telefon);
                rows++;
            }
            if (ansprechpartner.Fax != null && ansprechpartner.Fax != "")
            {
                right.Add("Fax: " + ansprechpartner.Fax);
                rows++;
            }
            if (ansprechpartner.Email != null && ansprechpartner.Email != "")
            {
                right.Add("E-Mail: " + ansprechpartner.Email);
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
        private static void ExplainUmlageschluessel(Betriebskostenabrechnung abrechnung, IPrint<T> printImpl)
        {
            var left1 = new List<string> { "Umlageschlüssel" };
            var right1 = new List<string> { "Bedeutung" };
            var left2 = new List<string> { "Umlageweg" };
            var right2 = new List<string> { "Beschreibung" };

            if (DirekteZuordnung(abrechnung.Abrechnungseinheiten))
            {
                left1.Add("Direkt");
                left2.Add("Direkt");
                right1.Add("Direkte Zuordnung");
                right2.Add("Kostenanteil = Kosten werden Einheit direkt zugeordnet.");
            }

            if (NachWohnflaeche(abrechnung.Abrechnungseinheiten))
            {
                left1.Add("n. WF.");
                left2.Add("n. WF.");
                right1.Add("nach Wohnfläche in m²");
                right2.Add("Kostenanteil = Kosten je Quadratmeter Wohnfläche mal Anteil Fläche je Wohnung.");
            }

            // There is a Umlage nach Nutzfläche in the Heizkostenberechnung:
            if (NachNutzflaeche(abrechnung.Abrechnungseinheiten))
            {
                left1.Add("n. NF");
                left2.Add("n. NF");
                right1.Add("nach Nutzfläche in m²");
                right2.Add("Kostenanteil = Kosten je Quadratmeter Nutzfläche mal Anteil Fläche je Wohnung.");
            }

            if (NachNutzeinheiten(abrechnung.Abrechnungseinheiten))
            {
                left1.Add("n. NE");
                left2.Add("n. NE");
                right1.Add("nach Anzahl der Wohn-/Nutzeinheiten");
                right2.Add("Kostenanteil = Kosten je Wohn-/Nutzeinheit.");
            }

            if (NachPersonenzahl(abrechnung.Abrechnungseinheiten))
            {
                left1.Add("n. Pers.");
                left2.Add("n. Pers.");
                right1.Add("nach Personenzahl/Anzahl der Bewohner");
                right2.Add("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung.");
            }

            if (NachVerbrauch(abrechnung.Abrechnungseinheiten))
            {
                left1.Add("n. Verb.");
                left2.Add("n. Verb.");
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
        private static void ExplainKalteBetriebskosten(Betriebskostenabrechnung abrechnung, IPrint<T> printImpl)
        {
            var runs = abrechnung.Vertrag.Wohnung.Umlagen
                .Where(r => r.Beschreibung != null && r.Beschreibung.Trim() != "")
                .SelectMany(t => new List<PrintRun>()
                {
                    new PrintRun(t.Typ.Bezeichnung + ": ") { Bold = true, NoBreak = true },
                    new PrintRun(t.Beschreibung ?? "")
                })
                .ToArray();

            printImpl.Paragraph(runs);
        }
        private static void AbrechnungWohnung(
            Betriebskostenabrechnung abrechnung,
            Abrechnungseinheit abrechnungseinheit,
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

            var anteile = abrechnungseinheit.PersonenZeitanteile.Where(anteile => anteile.Personenzahl > 0);
            foreach (var anteil in anteile)
            {
                var firstLine = anteil.Beginn == abrechnung.Zeitraum.Nutzungsbeginn;

                col1.Add(firstLine ? 1.ToString() : "");
                col2.Add(firstLine ? Quadrat(abrechnung.Vertrag.Wohnung.Wohnflaeche) : "");
                col3.Add(firstLine ? Quadrat(abrechnung.Vertrag.Wohnung.Nutzflaeche) : "");
                col4.Add(anteil.Personenzahl.ToString());
                col5.Add(Datum(anteil.Beginn) + " - " + Datum(anteil.Ende));
                col6.Add(anteil.Tage + "/" + abrechnung.Zeitraum.Abrechnungszeitraum);
            }

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }.Select(w => w.ToArray()).ToArray();
            var bold = Enumerable.Repeat(false, widths.Length).ToArray();
            bold[0] = true;
            var underlined = Enumerable.Repeat(false, widths.Length).ToArray();
            var justification = Enumerable.Repeat(1, widths.Length).ToArray();

            printImpl.Table(widths, justification, bold, underlined, cols); ;
        }
        private static void AbrechnungEinheit(Betriebskostenabrechnung abrechnung, Abrechnungseinheit abrechnungseinheit, IPrint<T> printImpl)
        {
            printImpl.SubHeading("Angaben zur Abrechnungseinheit:");
            printImpl.Text(abrechnungseinheit.Bezeichnung);

            var widths = new int[] { 14, 19, 19, 12, 28, 8 };
            var col1 = new List<string> { "Nutzeinheiten" };
            var col2 = new List<string> { "Wohnfläche" };
            var col3 = new List<string> { "Nutzfläche" };
            var col4 = new List<string> { "Bewohner" };
            var col5 = new List<string> { "Nutzungsintervall" };
            var col6 = new List<string> { "Tage" };

            var anteile = abrechnungseinheit.PersonenZeitanteile;
            for (var i = 0; i < anteile.Count; ++i)
            {
                var firstLine = i == 0;
                var anteil = anteile[i];

                var timespan = anteil.Tage.ToString();

                col1.Add(firstLine ? abrechnungseinheit.GesamtEinheiten.ToString() : "");
                col2.Add(firstLine ? Quadrat(abrechnungseinheit.GesamtWohnflaeche) : "");
                col3.Add(firstLine ? Quadrat(abrechnungseinheit.GesamtNutzflaeche) : "");
                col4.Add(anteil.GesamtPersonenzahl.ToString());
                col5.Add(Datum(anteil.Beginn) + " - " + Datum(anteil.Ende));
                col6.Add(timespan + "/" + abrechnung.Zeitraum.Abrechnungszeitraum);
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
            Betriebskostenabrechnung abrechnung,
            Abrechnungseinheit abrechnungseinheit,
            IPrint<T> printImpl)
        {
            var widths = new int[] { 41, 25, 17, 17 };
            var col1 = new List<string> { "Ermittlung Ihrer Anteile" };
            var col2 = new List<string> { "Nutzungsintervall" };
            var col3 = new List<string> { "Tage" };
            var col4 = new List<string> { "Ihr Anteil" };
            var bold = new List<bool> { true };
            var underlined = new List<bool> { false };

            if (abrechnungseinheit.GesamtEinheiten == 1)
            {
                col1.Add("Direkte Zuordnung");
                col2.Add(Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende));
                col3.Add(abrechnung.Zeitraum.Nutzungszeitraum.ToString() + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                col4.Add(Prozent(abrechnungseinheit.WFZeitanteil));
                bold.Add(false);
                underlined.Add(true);
            }
            else
            {
                if (abrechnungseinheit.Rechnungen
                    .Any(rechnung => rechnung.Key.Schluessel == Umlageschluessel.NachWohnflaeche))
                {
                    col1.Add("bei Umlage nach Wohnfläche (n. WF)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(Quadrat(abrechnung.Vertrag.Wohnung.Wohnflaeche) + " / " + Quadrat(abrechnungseinheit.GesamtWohnflaeche));
                    col2.Add(Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende));
                    col3.Add(abrechnung.Zeitraum.Nutzungszeitraum.ToString() + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                    col4.Add(Prozent(abrechnungseinheit.WFZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (abrechnungseinheit.Rechnungen
                    .Any(rechnung => rechnung.Key.Schluessel == Umlageschluessel.NachNutzflaeche))
                {
                    col1.Add("bei Umlage nach Nutzfläche (n. NF)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(Quadrat(abrechnung.Vertrag.Wohnung.Nutzflaeche) + " / " + Quadrat(abrechnungseinheit.GesamtNutzflaeche));
                    col2.Add(Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende));
                    col3.Add(abrechnung.Zeitraum.Nutzungszeitraum.ToString() + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                    col4.Add(Prozent(abrechnungseinheit.NFZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (abrechnungseinheit.Rechnungen
                    .Any(rechnung => rechnung.Key.Schluessel == Umlageschluessel.NachNutzeinheit))
                {
                    col1.Add("bei Umlage nach Nutzeinheiten (n. NE)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    col1.Add(abrechnung.Vertrag.Wohnung.Nutzeinheit + " / " + abrechnungseinheit.GesamtEinheiten);
                    col2.Add(Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende));
                    col3.Add(abrechnung.Zeitraum.Nutzungszeitraum.ToString() + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                    col4.Add(Prozent(abrechnungseinheit.NEZeitanteil));
                    bold.Add(false);
                    underlined.Add(true);
                }

                if (abrechnungseinheit.Rechnungen
                    .Any(rechnung => rechnung.Key.Schluessel == Umlageschluessel.NachPersonenzahl))
                {
                    col1.Add("bei Umlage nach Personenzahl (n. Pers.)");
                    col2.Add("");
                    col3.Add("");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    static string SingularOrPluralPerson(int i) => i.ToString() + (i > 1 ? " Personen" : " Person");
                    var personenzeitanteile = abrechnungseinheit.PersonenZeitanteile;
                    foreach (var personenzeitanteil in personenzeitanteile)
                    {
                        var Personenzahl = personenzeitanteil.Personenzahl;

                        if (Personenzahl == 0)
                        {
                            continue;
                        }

                        var Beginn = personenzeitanteil.Beginn;
                        var Ende = personenzeitanteil.Ende;
                        var GesamtPersonenzahl = personenzeitanteil.GesamtPersonenzahl;
                        var timespan = personenzeitanteil.Tage.ToString();

                        col1.Add(SingularOrPluralPerson(Personenzahl) + " / " + SingularOrPluralPerson(GesamtPersonenzahl));
                        col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                        col3.Add(timespan + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                        col4.Add(Prozent(personenzeitanteil.Anteil));
                        bold.Add(false);
                        underlined.Add(personenzeitanteil == personenzeitanteile.Last());
                    }
                }

                if (abrechnungseinheit.Rechnungen
                    .Any(rechnung => rechnung.Key.Schluessel == Umlageschluessel.NachVerbrauch))
                {
                    col1.Add("bei Umlage nach Verbrauch (n. Verb.)");
                    col2.Add("");
                    col3.Add("Zählernummer");
                    col4.Add("");
                    bold.Add(true);
                    underlined.Add(false);

                    var verbrauchAnteile = abrechnungseinheit.VerbrauchAnteile;
                    // Kalte Betriebskosten are equal / warme are odd
                    var kalteAnteile = verbrauchAnteile.Where(verbrauch => verbrauch.Umlage.HKVO == null);
                    foreach (var verbrauchAnteil in kalteAnteile)
                    {
                        foreach (var zaehlerTyp in verbrauchAnteil.DieseZaehler)
                        {
                            var typ = zaehlerTyp.Key;
                            var unit = typ.ToUnitString();
                            var deltaAll = verbrauchAnteil.AlleZaehler[typ].Sum(e => e.Delta);
                            var value = zaehlerTyp.Value;

                            for (var i = 0; i < value.Count; ++i)
                            {
                                var delta = value[i].Delta;
                                col1.Add(Unit(delta, unit) + " / " + Unit(deltaAll, unit) + " (" + verbrauchAnteil.Umlage.Typ.Bezeichnung + ")");
                                col2.Add("");
                                col3.Add(value[i].Zaehler.Kennnummer);
                                col4.Add(Prozent(delta / deltaAll));
                                bold.Add(false);
                                underlined.Add(false);
                            }
                            var deltaHere = verbrauchAnteil.DieseZaehler[typ].Sum(e => e.Delta);
                            col1.Add(Unit(deltaHere, unit) + " / " + Unit(deltaAll, unit));
                            col2.Add(Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende));
                            col3.Add(abrechnung.Zeitraum.Nutzungszeitraum.ToString() + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                            col4.Add(Prozent(verbrauchAnteil.Anteil[typ]));
                            bold.Add(true);
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
            Betriebskostenabrechnung abrechnung,
            Abrechnungseinheit abrechnungseinheit,
            IPrint<T> printImpl)
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

            void kostenPunkt(Umlage umlage, string zeitraum, int Jahr, double anteil, bool firstLine = true)
            {
                var betrag = umlage.Betriebskostenrechnungen
                    .Where(rechnung => rechnung.BetreffendesJahr == abrechnung.Zeitraum.Jahr)
                    .Sum(b => b.Betrag);
                col1.Add(firstLine ? umlage.Typ.Bezeichnung : "");
                col2.Add(abrechnungseinheit.GesamtEinheiten == 1 ? "Direkt" : (firstLine ? umlage.Schluessel.ToDescriptionString() : ""));
                col3.Add(zeitraum);
                col4.Add(Euro(betrag));
                col5.Add(Prozent(anteil));
                col6.Add(Euro(betrag * anteil));
                bold.Add(false);
                underlined.Add(true);
            }

            var personenzeitanteile = abrechnungseinheit.PersonenZeitanteile;

            // TODO use rechnungen directly instead of going back and forth.
            var umlagen = abrechnungseinheit.Rechnungen
                .Select(rechnung => rechnung.Key)
                .Where(umlage => umlage.HKVO == null);
            foreach (var umlage in umlagen)
            {
                switch (umlage.Schluessel)
                {
                    case Umlageschluessel.NachWohnflaeche:
                        kostenPunkt(
                            umlage,
                            Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende),
                            abrechnung.Zeitraum.Jahr,
                            abrechnungseinheit.WFZeitanteil);
                        break;
                    case Umlageschluessel.NachNutzeinheit:
                        kostenPunkt(
                            umlage,
                            Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende),
                            abrechnung.Zeitraum.Jahr,
                            abrechnungseinheit.NEZeitanteil);
                        break;
                    case Umlageschluessel.NachPersonenzahl:
                        var first = true;
                        if (personenzeitanteile.All(e => e.Personenzahl == 0))
                        {
                            kostenPunkt(
                                umlage,
                                Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende),
                                abrechnung.Zeitraum.Jahr,
                                0,
                                first);
                        }
                        else
                        {
                            foreach (var a in personenzeitanteile)
                            {
                                if (a.Personenzahl == 0)
                                {
                                    continue;
                                }
                                kostenPunkt(
                                    umlage,
                                    Datum(a.Beginn) + " - " + Datum(a.Ende),
                                    abrechnung.Zeitraum.Jahr,
                                    a.Anteil,
                                    first);
                                first = false;
                            }
                        }
                        break;
                    case Umlageschluessel.NachVerbrauch:
                        var anteil = abrechnungseinheit.VerbrauchAnteile.Single(e => e.Umlage == umlage);
                        // TODO anteil can only have one ZaehlerTyp. Catch that?

                        kostenPunkt(
                            umlage,
                            Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende),
                            abrechnung.Zeitraum.Jahr,
                            anteil.Anteil.First().Value);
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
            col6.Add(Euro(abrechnungseinheit.BetragKalt));
            bold.Add(true);
            underlined.Add(false);

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6 }
                .Select(w => w.ToArray())
                .ToArray();

            var justification = new int[] { 0, 0, 1, 2, 2, 2 };

            printImpl.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        private static void ErmittlungWarmeKosten(Betriebskostenabrechnung abrechnung, Abrechnungseinheit abrechnungseinheit, IPrint<T> p)
        {
            var widths = new int[] { 50, 10 };

            // TODO use rechnungen directly instead of going back and forth.
            var umlagen = abrechnungseinheit.Rechnungen
                .Select(rechnung => rechnung.Key)
                .Where(umlage => umlage.HKVO != null);
            foreach (var umlage in umlagen)
            {
                var betrag = umlage.Betriebskostenrechnungen
                    .Where(rechnung => rechnung.BetreffendesJahr == abrechnung.Zeitraum.Jahr)
                    .Sum(b => b.Betrag);
                var col1 = new List<string>
                {
                    umlage.Typ.Bezeichnung,
                    "Kosten für Brennstoffe",
                    "Betriebskosten der Anlage (5% pauschal)",
                    "Gesamt",
                };
                var col2 = new List<string>
                {
                    "Betrag",
                    Euro(betrag),
                    Euro(abrechnungseinheit.AllgStromFaktor),
                    Euro(betrag + abrechnungseinheit.AllgStromFaktor),
                };
                var cols = new List<List<string>> { col1, col2 }.Select(w => w.ToArray()).ToArray();

                var justification = new int[] { 0, 2 };
                var bold = new bool[] { true, false, false, true };
                var underlined = new bool[] { false, false, false, false };

                p.Table(widths, justification, bold, underlined, cols);
            }
        }
        private static void ErmittlungWarmeEinheiten(Betriebskostenabrechnung abrechnung, Abrechnungseinheit abrechnungseinheit, IPrint<T> p)
        {
            var widths = new int[] { 41, 25, 17, 17 };
            var col1 = new List<string> { "Ermittlung Ihrer Einheiten" };
            var col2 = new List<string> { "Nutzungsintervall" };
            var col3 = new List<string> { "Tage" };
            var col4 = new List<string> { "Ihr Anteil" };

            col1.Add("bei Umlage nach Nutzfläche (n. NF)");
            col2.Add("");
            col3.Add("");
            col4.Add("");

            col1.Add(Quadrat(abrechnung.Vertrag.Wohnung.Nutzflaeche) + " / " + Quadrat(abrechnungseinheit.GesamtNutzflaeche));
            col2.Add(Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende));
            col3.Add(abrechnung.Zeitraum.Nutzungszeitraum.ToString() + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
            col4.Add(Prozent(abrechnungseinheit.NFZeitanteil));

            var bold = new List<bool> { true, true, false };
            var underlined = new List<bool> { false, false, true };

            var warmeRechnungen = abrechnungseinheit.Rechnungen
                .Select(rechnung => rechnung.Key)
                .Where(umlage => umlage.HKVO != null).ToList();

            if (warmeRechnungen.Exists(umlage => umlage.Schluessel == Umlageschluessel.NachPersonenzahl))
            {
                col1.Add("bei Umlage nach Personenzahl (n. Pers.)");
                col2.Add("");
                col3.Add("");
                col4.Add("");

                bold.Add(true);
                bold.Add(false);

                static string SingularOrPluralPerson(int i) => i.ToString() + (i > 1 ? " Personen" : " Person");
                var personenzeitanteile = abrechnungseinheit.PersonenZeitanteile;
                foreach (var personenzeitanteil in personenzeitanteile)
                {
                    var Personenzahl = personenzeitanteil.Personenzahl;

                    if (Personenzahl == 0)
                    {
                        continue;
                    }

                    var Beginn = personenzeitanteil.Beginn;
                    var Ende = personenzeitanteil.Ende;
                    var GesamtPersonenzahl = personenzeitanteil.GesamtPersonenzahl;

                    var timespan = (Ende.DayNumber - Beginn.DayNumber + 1).ToString();

                    if (personenzeitanteil == personenzeitanteile.Last())
                    {

                        col1.Add(SingularOrPluralPerson(Personenzahl) + " / " + SingularOrPluralPerson(GesamtPersonenzahl));
                        col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                        col3.Add(timespan + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                        col4.Add(Prozent(personenzeitanteil.Anteil));
                    }
                    else
                    {
                        col1.Add(SingularOrPluralPerson(Personenzahl) + " / " + SingularOrPluralPerson(GesamtPersonenzahl));
                        col2.Add(Datum(Beginn) + " - " + Datum(Ende));
                        col3.Add(timespan + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                        col4.Add(Prozent(personenzeitanteil.Anteil));
                    }
                    bold.Add(false);
                    underlined.Add(personenzeitanteil == personenzeitanteile.Last());
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

                var verbrauchAnteile = abrechnungseinheit.VerbrauchAnteile;
                // Kalte Betriebskosten are equal / warme are odd
                var warmeAnteile = verbrauchAnteile.Where(verbrauch => verbrauch.Umlage.HKVO != null);
                foreach (var verbrauchAnteil in warmeAnteile)
                {
                    foreach (var zaehlerTyp in verbrauchAnteil.DieseZaehler)
                    {
                        var typ = zaehlerTyp.Key;
                        var unit = typ.ToUnitString();
                        var deltaAll = verbrauchAnteil.AlleZaehler[typ].Sum(e => e.Delta);
                        var value = zaehlerTyp.Value;

                        for (var i = 0; i < value.Count; ++i)
                        {
                            var delta = value[i].Delta;
                            col1.Add(Unit(delta, unit) + " / " + Unit(deltaAll, unit));
                            col2.Add("");
                            col3.Add(value[i].Zaehler.Kennnummer);
                            col4.Add(Prozent(delta / deltaAll));
                            bold.Add(false);
                            underlined.Add(false);
                        }
                        var deltaHere = verbrauchAnteil.DieseZaehler[typ].Sum(e => e.Delta);
                        col1.Add(Unit(deltaHere, unit) + " / " + Unit(deltaAll, unit));
                        col2.Add(Datum(abrechnung.Zeitraum.Nutzungsbeginn) + " - " + Datum(abrechnung.Zeitraum.Nutzungsende));
                        col3.Add(abrechnung.Zeitraum.Nutzungszeitraum.ToString() + " / " + abrechnung.Zeitraum.Abrechnungszeitraum.ToString());
                        col4.Add(Prozent(verbrauchAnteil.Anteil[typ]));
                        bold.Add(true);
                        underlined.Add(true);
                    }
                }
            }

            var cols = new List<List<string>> { col1, col2, col3, col4 }.Select(w => w.ToArray()).ToArray();
            var justification = new int[] { 0, 1, 1, 1 };

            p.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        private static void ErmittlungWarmanteil(Betriebskostenabrechnung abrechnung, Abrechnungseinheit abrechnungseinheit, IPrint<T> p)
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

            var wohnung = abrechnung.Vertrag.Wohnung;

            foreach (var heizkostenberechnung in abrechnungseinheit.Heizkostenberechnungen)
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
                col7.Add(Euro(abrechnungseinheit.BetragWarm));
                bold.Add(true);
                underlined.Add(false);
            }

            var cols = new List<List<string>> { col1, col2, col3, col4, col5, col6, col7 }.Select(w => w.ToArray()).ToArray();
            var justification = new int[] { 0, 0, 0, 1, 1, 1, 2 };

            p.Table(widths, justification, bold.ToArray(), underlined.ToArray(), cols);
        }
        private static void GesamtErgebnis(Betriebskostenabrechnung abrechnung, IPrint<T> p)
        {
            var widths = new int[] { 40, 20 };

            var col1 = new List<string>
            {
                "Sie haben vorausgezahlt:"
            };

            var col2 = new List<string>
            {
                Euro(abrechnung.GezahlteMiete - abrechnung.KaltMiete)
            };

            var f = true;
            foreach (var abrechnungseinheit in abrechnung.Abrechnungseinheiten)
            {
                if (abrechnungseinheit.BetragKalt > 0)
                {
                    col1.Add(f ? "Abzüglich Ihrer Nebenkostenanteile: " : "");
                    col2.Add("-" + Euro(abrechnungseinheit.BetragKalt));
                    f = false;
                }
            }

            foreach (var abrechnungseinheit in abrechnung.Abrechnungseinheiten)
            {
                var wohnung = abrechnung.Vertrag.Wohnung;
                if (abrechnungseinheit.BetragWarm > 0)
                {
                    col1.Add(f ? "Abzüglich Ihrer Nebenkostenanteile: " : "");
                    col2.Add("-" + Euro(abrechnungseinheit.BetragWarm));
                    f = false;
                }
            }

            if (abrechnung.Mietminderung > 0)
            {
                col1.Add("Verrechnung mit Mietminderung: ");
                col2.Add("+" + Euro(abrechnung.NebenkostenMietminderung));
            }

            col1.Add("Ergebnis:");
            col2.Add(Euro(abrechnung.Result));

            var cols = new List<List<string>> { col1, col2 }.Select(w => w.ToArray()).ToArray();
            var justification = new int[] { 0, 2 };
            var bold = Enumerable.Repeat(false, col1.Count).ToArray();
            bold[^1] = true;
            var underlined = Enumerable.Repeat(true, col1.Count).ToArray();
            underlined[^1] = false;

            p.Table(widths, justification, bold, underlined, cols);
        }

        private static void Introtext(Betriebskostenabrechnung abrechnung, IPrint<T> p)
        {
            p.Paragraph(
                new PrintRun(Title(abrechnung.Zeitraum.Jahr)) { Bold = true },
                new PrintRun(Mieterliste(abrechnung.Mieter)),
                new PrintRun(Mietobjekt(abrechnung.Vertrag.Wohnung)),
                new PrintRun("Abrechnungszeitraum: ") { NoBreak = true, Tab = true },
                new PrintRun(Abrechnungszeitraum(abrechnung.Zeitraum)),
                new PrintRun("Nutzungszeitraum: ") { NoBreak = true, Tab = true },
                new PrintRun(Nutzungszeitraum(abrechnung.Zeitraum)));

            p.Paragraph(
                new PrintRun(Gruss(abrechnung.Mieter)),
                new PrintRun(ResultTxt(abrechnung.Result)) { NoBreak = true, Tab = true },
                new PrintRun(Euro(Math.Abs(abrechnung.Result))) { Bold = true, Underlined = true },
                new PrintRun(RefundDemand(abrechnung.Result)));

            p.Paragraph(new PrintRun(GenerischerText(abrechnung.Abrechnungseinheiten)));
        }

        public static T Print(Betriebskostenabrechnung abrechnung, IPrint<T> printImpl)
        {
            Header(abrechnung, printImpl);
            Introtext(abrechnung, printImpl);
            printImpl.PageBreak();

            printImpl.Heading("Abrechnung der Nebenkosten");
            ExplainUmlageschluessel(abrechnung, printImpl);
            printImpl.Break();
            printImpl.Text("Anmerkung:");
            printImpl.Text(Anmerkung);
            printImpl.Heading("Erläuterungen zu einzelnen Betriebskostenarten");
            ExplainKalteBetriebskosten(abrechnung, printImpl);

            printImpl.PageBreak();

            if (abrechnung.Abrechnungseinheiten.FirstOrDefault() is Abrechnungseinheit einheit)
            {
                AbrechnungWohnung(abrechnung, einheit, printImpl);
            }

            printImpl.Break();
            printImpl.Heading("Abrechnung der Nebenkosten (kalte Betriebskosten)");

            foreach (var abrechnungseinheit in abrechnung.Abrechnungseinheiten.Where(einheit => einheit.GesamtEinheiten == 1))
            {
                AbrechnungEinheit(abrechnung, abrechnungseinheit, printImpl);
                printImpl.Break();
                ErmittlungKalteEinheiten(abrechnung, abrechnungseinheit, printImpl);
                printImpl.Break();

                ErmittlungKalteKosten(abrechnung, abrechnungseinheit, printImpl);
            }
            printImpl.Break();

            foreach (var abrechnungseinheit in abrechnung.Abrechnungseinheiten.Where(einheit => einheit.GesamtEinheiten > 1))
            {
                AbrechnungEinheit(abrechnung, abrechnungseinheit, printImpl);
                printImpl.Break();
                ErmittlungKalteEinheiten(abrechnung, abrechnungseinheit, printImpl);
                printImpl.Break();

                ErmittlungKalteKosten(abrechnung, abrechnungseinheit, printImpl);
            }

            printImpl.PageBreak();
            printImpl.Heading("Abrechnung der Nebenkosten (warme Betriebskosten)");

            foreach (var abrechnungseinheit in abrechnung.Abrechnungseinheiten)
            {
                if (abrechnungseinheit.GesamtBetragWarm > 0)
                {
                    if (abrechnungseinheit.GesamtEinheiten == 1)
                    {
                        printImpl.SubHeading("Direkt zugeordnet:");
                        ErmittlungWarmeKosten(abrechnung, abrechnungseinheit, printImpl);
                    }
                    else
                    {
                        AbrechnungEinheit(abrechnung, abrechnungseinheit, printImpl);
                        printImpl.Break();
                        ErmittlungWarmeKosten(abrechnung, abrechnungseinheit, printImpl);
                        printImpl.EqHeizkostenV9_2(abrechnung, abrechnungseinheit);
                        ErmittlungWarmeEinheiten(abrechnung, abrechnungseinheit, printImpl);
                        printImpl.Break();
                        printImpl.SubHeading("Ermittlung der warmen Betriebskosten");
                        ErmittlungWarmanteil(abrechnung, abrechnungseinheit, printImpl);
                    }
                }
            }

            printImpl.Heading("Gesamtergebnis der Abrechnung");
            GesamtErgebnis(abrechnung, printImpl);

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
