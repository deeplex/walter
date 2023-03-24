namespace Deeplex.Saverwalter.Model
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

        public static string GetBriefAnrede(this IPerson p)
        {
            var ret = "";
            if (p is NatuerlichePerson n)
            {
                ret += n.Anrede == Anrede.Herr ? "Herrn " :
                    n.Anrede == Anrede.Frau ? "Frau " :
                    "";
            }
            ret += p.Bezeichnung;

            return ret;
        }

        public static string Title(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
            => "Betriebskostenabrechnung " + b.Jahr.ToString();

        public static string Mieterliste(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
            => "Mieter: " + string.Join(", ", b.Mieter.Select(m => m.Bezeichnung));

        public static string Mietobjekt(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
            => "Mietobjekt: " + b.Adresse.Strasse + " " + b.Adresse.Hausnummer + ", " + b.Wohnung.Bezeichnung;

        public static string Abrechnungszeitraum(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
            => b.Abrechnungsbeginn.ToString("dd.MM.yyyy") + " - " + b.Abrechnungsende.ToString("dd.MM.yyyy");

        public static string Nutzungszeitraum(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
            => b.Nutzungsbeginn.ToString("dd.MM.yyyy") + " - " + b.Nutzungsende.ToString("dd.MM.yyyy");

        public static string Gruss(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
        {
            var gruss = b.Mieter.Aggregate("", (r, m) =>
            {
                if (m is NatuerlichePerson n)
                {
                    return r + (n.Anrede == Anrede.Herr ? "sehr geehrter Herr " :
                        n.Anrede == Anrede.Frau ? "sehr geehrte Frau " :
                        n.Vorname) + n.Nachname + ", ";
                }
                else
                {
                    return "Sehr geehrte Damen und Herren, ";
                }
            });

            return gruss.Remove(1).ToUpper() + gruss.Substring(1);
        }

        public static string ResultTxt(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
            => "wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. " +
                "Die Abrechnung schließt mit " + (b.Result > 0 ?
                "einem Guthaben" : "einer Nachforderung") + " in Höhe von: ";

        public static string RefundDemand(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
            => b.Result > 0 ?
            "Dieser Betrag wird über die von Ihnen angegebene Bankverbindung erstattet." :
            "Bitte überweisen Sie diesen Betrag auf das Ihnen bekannte Konto.";

        public static string GenerischerText(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
        {
            // TODO Text auf Anwesenheit von Heizung oder so testen und anpassen.

            var t = "Die Abrechnung betrifft zunächst die mietvertraglich vereinbarten Nebenkosten (die kalten Betriebskosten). ";

            if (b.Gruppen.Any(g => g.GesamtBetragWarm != 0 && g.BetragWarm != 0))
            {
                t += "Die Kosten für die Heizung und für die Erwärmung von Wasser über die Heizanlage Ihres Wohnhauses (warme Betriebskosten) " +
                "werden gesondert berechnet, nach Verbrauch und Wohn -/ Nutzfläche auf die einzelnen Wohnungen " +
                "umgelegt („Ihre Heizungsrechnung“) und mit dem Ergebnis aus der Aufrechnung Ihrer Nebenkosten und der Summe der " +
                "von Ihnen geleisteten Vorauszahlungen verrechnet. ";
            }

            t += "Bei bestehenden Mietrückständen ist das Ergebnis der Abrechnung " +
                "zusätzlich mit den Mietrückständen verrechnet. Gegebenenfalls bestehende Mietminderungen / Ratenzahlungsvereinbarungen " +
                "sind hier nicht berücksichtigt, haben aber weiterhin für den vereinbarten Zeitraum Bestand. Aufgelöste oder gekündigte " +
                "Mietverhältnisse werden durch dieses Schreiben nicht neu begründet. Die Aufstellung, Verteilung und Erläuterung der " +
                "Gesamtkosten, die Berechnung der Kostenanteile, die Verrechnung der geleisteten Vorauszahlungen und gegebenenfalls die " +
                "Neuberechnung der monatlichen Vorauszahlungen entnehmen Sie bitte den folgenden Seiten.";

            return t;
        }

        public static bool direkteZuordnung(this BetriebskostenabrechnungService.Betriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.Gruppen.Any(rechnungsgruppe => rechnungsgruppe.Umlagen.Any(umlage => umlage.Wohnungen.Count == 1));

        private static bool umlageSchluesselExistsInBetriebskostenabrechnung(this BetriebskostenabrechnungService.Betriebskostenabrechnung betriebskostenrechnung, Umlageschluessel umlageSchluessel) =>
            betriebskostenrechnung.Gruppen
                .Any(rechnungsgruppe => rechnungsgruppe.Umlagen
                    .Where(umlage => umlage.Wohnungen.Count > 1)
                    .Any(umlage => umlage.Schluessel == umlageSchluessel));

        public static bool nachWohnflaeche(this BetriebskostenabrechnungService.Betriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.umlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachWohnflaeche);
        public static bool nachNutzflaeche(this BetriebskostenabrechnungService.Betriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.umlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachNutzflaeche);
        public static bool nachNutzeinheiten(this BetriebskostenabrechnungService.Betriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.umlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachNutzeinheit);
        public static bool nachPersonenzahl(this BetriebskostenabrechnungService.Betriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.umlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachPersonenzahl);
        public static bool nachVerbrauch(this BetriebskostenabrechnungService.Betriebskostenabrechnung betriebskostenabrechnung)
            => betriebskostenabrechnung.umlageSchluesselExistsInBetriebskostenabrechnung(Umlageschluessel.NachVerbrauch);

        public static string Anmerkung(this BetriebskostenabrechnungService.Betriebskostenabrechnung b)
            => "Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger) Kostenanteil";

    }
}
