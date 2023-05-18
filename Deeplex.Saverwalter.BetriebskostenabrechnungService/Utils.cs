using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public static class Utils
    {
        public static T Max<T>(T l, T r) where T : IComparable<T>
            => Max(l, r, Comparer<T>.Default);
        public static T Max<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) < 0 ? r : l;

        public static T Min<T>(T l, T r) where T : IComparable<T>
            => Min(l, r, Comparer<T>.Default);
        public static T Min<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) > 0 ? r : l;

        public static string GetBriefAnrede(this IPerson person)
        {
            var text = "";
            if (person is NatuerlichePerson natuerlichePerson)
            {
                text += natuerlichePerson.Anrede == Anrede.Herr ? "Herrn " :
                    natuerlichePerson.Anrede == Anrede.Frau ? "Frau " :
                    "";
            }
            text += person.Bezeichnung;

            return text;
        }

        public static string Title(this IBetriebskostenabrechnung betriebskostenabrechnung)
            => "Betriebskostenabrechnung " + betriebskostenabrechnung.Zeitraum.Jahr.ToString();

        public static string Mieterliste(this IBetriebskostenabrechnung betriebskostenabrechnung)
            => "Mieter: " + string.Join(", ", betriebskostenabrechnung.Mieter.Select(person => person.Bezeichnung));

        public static string Mietobjekt(this IBetriebskostenabrechnung betriebskostenabrechnung)
            => "Mietobjekt: " + betriebskostenabrechnung.Adresse.Strasse + " " + betriebskostenabrechnung.Adresse.Hausnummer + ", " + betriebskostenabrechnung.Wohnung.Bezeichnung;

        public static string Abrechnungszeitraum(this IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.Zeitraum.Abrechnungsbeginn.ToString("dd.MM.yyyy") + " - " + betriebskostenabrechnung.Zeitraum.Abrechnungsende.ToString("dd.MM.yyyy");

        public static string Nutzungszeitraum(this IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.Zeitraum.Nutzungsbeginn.ToString("dd.MM.yyyy") + " - " + betriebskostenabrechnung.Zeitraum.Nutzungsende.ToString("dd.MM.yyyy");

        public static string Gruss(this IBetriebskostenabrechnung betriebskostenabrechnung)
        {
            var gruss = betriebskostenabrechnung.Mieter.Aggregate("", (text, mieter) =>
            {
                if (mieter is NatuerlichePerson natuerlichePerson)
                {
                    return text + (natuerlichePerson.Anrede == Anrede.Herr ? "sehr geehrter Herr " :
                        natuerlichePerson.Anrede == Anrede.Frau ? "sehr geehrte Frau " :
                        natuerlichePerson.Vorname) + natuerlichePerson.Nachname + ", ";
                }
                else
                {
                    return "Sehr geehrte Damen und Herren, ";
                }
            });

            return gruss.Remove(1).ToUpper() + gruss[1..];
        }

        public static string ResultTxt(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
            => "wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. Die Abrechnung schließt mit " + (betriebskostenabrechnung.Result > 0 ? "einem Guthaben" : "einer Nachforderung") + " in Höhe von: ";

        public static string RefundDemand(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.Result > 0 ?
            "Dieser Betrag wird über die von Ihnen angegebene Bankverbindung erstattet." :
            "Bitte überweisen Sie diesen Betrag auf das Ihnen bekannte Konto.";

        public static string GenerischerText(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
        {
            // TODO Text auf Anwesenheit von Heizung oder so testen und anpassen.

            var text = "Die Abrechnung betrifft zunächst die mietvertraglich vereinbarten Nebenkosten (die kalten Betriebskosten). ";

            if (betriebskostenabrechnung.Abrechnungseinheiten.Any(rechnungsgruppe => rechnungsgruppe.GesamtBetragWarmeNebenkosten != 0 && rechnungsgruppe.BetragWarmeNebenkosten != 0))
            {
                text += "Die Kosten für die Heizung und für die Erwärmung von Wasser über die Heizanlage Ihres Wohnhauses (warme Betriebskosten) werden gesondert berechnet, nach Verbrauch und Wohn -/ Nutzfläche auf die einzelnen Wohnungen umgelegt („Ihre Heizungsrechnung“) und mit dem Ergebnis aus der Aufrechnung Ihrer Nebenkosten und der Summe der von Ihnen geleisteten Vorauszahlungen verrechnet.";
            }

            text += "Bei bestehenden Mietrückständen ist das Ergebnis der Abrechnung zusätzlich mit den Mietrückständen verrechnet. Gegebenenfalls bestehende Mietminderungen / Ratenzahlungsvereinbarungen sind hier nicht berücksichtigt, haben aber weiterhin für den vereinbarten Zeitraum Bestand. Aufgelöste oder gekündigte Mietverhältnisse werden durch dieses Schreiben nicht neu begründet. Die Aufstellung, Verteilung und Erläuterung der Gesamtkosten, die Berechnung der Kostenanteile, die Verrechnung der geleisteten Vorauszahlungen und gegebenenfalls die Neuberechnung der monatlichen Vorauszahlungen entnehmen Sie bitte den folgenden Seiten.";

            return text;
        }

        public static bool DirekteZuordnung(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.Abrechnungseinheiten.Any(rechnungsgruppe => rechnungsgruppe.Umlagen.Any(umlage => umlage.Wohnungen.Count == 1));

        private static bool UmlageSchluesselExistsInBetriebskostenabrechnung(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenrechnung, Umlageschluessel umlageSchluessel) =>
            betriebskostenrechnung.Abrechnungseinheiten
                .Any(rechnungsgruppe => rechnungsgruppe.Umlagen
                    .Where(umlage => umlage.Wohnungen.Count > 1)
                    .Any(umlage => umlage.Schluessel == umlageSchluessel));

        public static bool NachWohnflaeche(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.UmlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachWohnflaeche);
        public static bool NachNutzflaeche(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.UmlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachNutzflaeche);
        public static bool NachNutzeinheiten(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.UmlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachNutzeinheit);
        public static bool NachPersonenzahl(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.UmlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachPersonenzahl);
        public static bool NachVerbrauch(this BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.UmlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachVerbrauch);

        public static string Anmerkung(this BetriebskostenabrechnungService.IBetriebskostenabrechnung _)
            => "Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger) Kostenanteil";

    }
}
