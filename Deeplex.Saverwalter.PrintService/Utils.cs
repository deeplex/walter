using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.PrintService
{
    public static class Utils
    {
        public static string Anschrift(Adresse a) => a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;

        public static string Prozent(double d) => string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:N2}%", d * 100);
        public static string Euro(double d) => string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:N2}€", d);
        public static string Unit(double d, string unit) => string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:N2}" + unit, d);
        public static string Celsius(double d) => string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:N2}°C", d);
        public static string Celsius(int d) => Celsius((double)d);
        public static string Quadrat(double d) => string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:N2}m²", d);
        public static string Datum(DateOnly d) => d.ToString("dd.MM.yyyy");

        public static string GetBriefAnrede(Kontakt person)
        {
            var text = "";
            if (person.Rechtsform == Rechtsform.natuerlich)
            {
                text += person.Anrede == Anrede.Herr ? "Herrn " :
                    person.Anrede == Anrede.Frau ? "Frau " :
                    "";
            }
            text += person.Bezeichnung;

            return text;
        }

        public static string Title(int jahr)
            => "Betriebskostenabrechnung " + jahr.ToString();

        public static string Mieterliste(List<Kontakt> mieter)
            => "Mieter: " + string.Join(", ", mieter.Select(person => person.Bezeichnung));

        public static string Mietobjekt(Wohnung wohnung)
            => "Mietobjekt: " + wohnung.Adresse!.Strasse + " " + wohnung.Adresse.Hausnummer + ", " + wohnung.Bezeichnung;

        public static string Abrechnungszeitraum(Zeitraum zeitraum)
            => zeitraum.Abrechnungsbeginn.ToString("dd.MM.yyyy") + " - " + zeitraum.Abrechnungsende.ToString("dd.MM.yyyy");

        public static string Nutzungszeitraum(Zeitraum zeitraum)
            => zeitraum.Nutzungsbeginn.ToString("dd.MM.yyyy") + " - " + zeitraum.Nutzungsende.ToString("dd.MM.yyyy");

        public static string Gruss(List<Kontakt> mieter)
        {
            var gruss = mieter.Aggregate("", (text, mieter) =>
            {
                if (mieter.Rechtsform == Rechtsform.natuerlich)
                {
                    return text + (mieter.Anrede == Anrede.Herr ? "sehr geehrter Herr " :
                        mieter.Anrede == Anrede.Frau ? "sehr geehrte Frau " :
                        mieter.Vorname) + mieter.Name + ", ";
                }
                else
                {
                    return "Sehr geehrte Damen und Herren, ";
                }
            });

            return gruss.Remove(1).ToUpper() + gruss[1..];
        }

        public static string ResultTxt(double result)
           => $"wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. Die Abrechnung schließt mit {(result > 0 ? "einem Guthaben" : "einer Nachforderung")} in Höhe von: ";


        public const string RefundPositive
            = "Dieser Betrag wird über die von Ihnen angegebene Bankverbindung erstattet.";
        public const string RefundNegative
            = "Bitte überweisen Sie diesen Betrag auf das Ihnen bekannte Konto.";
        public static string RefundDemand(double result)
            => result > 0 ? RefundPositive : RefundNegative;

        public const string GenerischerTextFirstPart
            = "Die Abrechnung betrifft zunächst die mietvertraglich vereinbarten Nebenkosten (die kalten Betriebskosten). ";
        public const string GenerischerTextHeizungPart
            = "Die Kosten für die Heizung und für die Erwärmung von Wasser über die Heizanlage Ihres Wohnhauses (warme Betriebskosten) werden gesondert berechnet, nach Verbrauch und Wohn -/ Nutzfläche auf die einzelnen Wohnungen umgelegt („Ihre Heizungsrechnung“) und mit dem Ergebnis aus der Aufrechnung Ihrer Nebenkosten und der Summe der von Ihnen geleisteten Vorauszahlungen verrechnet.";
        public const string GenerischerTextFinalPart
            = "Bei bestehenden Mietrückständen ist das Ergebnis der Abrechnung zusätzlich mit den Mietrückständen verrechnet. Gegebenenfalls bestehende Mietminderungen / Ratenzahlungsvereinbarungen sind hier nicht berücksichtigt, haben aber weiterhin für den vereinbarten Zeitraum Bestand. Aufgelöste oder gekündigte Mietverhältnisse werden durch dieses Schreiben nicht neu begründet. Die Aufstellung, Verteilung und Erläuterung der Gesamtkosten, die Berechnung der Kostenanteile, die Verrechnung der geleisteten Vorauszahlungen und gegebenenfalls die Neuberechnung der monatlichen Vorauszahlungen entnehmen Sie bitte den folgenden Seiten.";

        public static string GenerischerText(Wohnung wohnung, List<Abrechnungseinheit> einheiten, Zeitraum zeitraum, List<Note> notes)
        {
            // TODO Text auf Anwesenheit von Heizung oder so testen und anpassen.

            var text = GenerischerTextFirstPart; ;

            if (einheiten.Any(abrechnungseinheit =>
                abrechnungseinheit.GesamtBetragWarm != 0 &&
                abrechnungseinheit.BetragWarm != 0))
            {
                text += GenerischerTextHeizungPart;
            }

            text += GenerischerTextFinalPart;

            return text;
        }

        public static bool DirekteZuordnung(List<Abrechnungseinheit> einheiten)
            => einheiten.Any(einheit => einheit.Rechnungen.Any(rechnung => rechnung.Key.Wohnungen.Count == 1));

        private static bool UmlageSchluesselExistsInabrechnung(List<Abrechnungseinheit> einheiten, Umlageschluessel umlageSchluessel) =>
            einheiten.Any(einheit => einheit.Rechnungen
                .Where(rechnung => rechnung.Key.Wohnungen.Count > 1)
                .Any(rechnung => rechnung.Key.Schluessel == umlageSchluessel));

        public static bool NachWohnflaeche(List<Abrechnungseinheit> einheiten)
            => UmlageSchluesselExistsInabrechnung(einheiten, Umlageschluessel.NachWohnflaeche);
        public static bool NachNutzflaeche(List<Abrechnungseinheit> einheiten)
            => UmlageSchluesselExistsInabrechnung(einheiten, Umlageschluessel.NachNutzflaeche);
        public static bool NachNutzeinheiten(List<Abrechnungseinheit> einheiten)
            => UmlageSchluesselExistsInabrechnung(einheiten, Umlageschluessel.NachNutzeinheit);
        public static bool NachPersonenzahl(List<Abrechnungseinheit> einheiten)
            => UmlageSchluesselExistsInabrechnung(einheiten, Umlageschluessel.NachPersonenzahl);
        public static bool NachVerbrauch(List<Abrechnungseinheit> einheiten)
            => UmlageSchluesselExistsInabrechnung(einheiten, Umlageschluessel.NachVerbrauch);

        public const string Anmerkung
           = "Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger) Kostenanteil";

    }
}
