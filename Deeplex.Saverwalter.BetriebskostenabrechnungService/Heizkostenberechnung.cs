namespace Deeplex.Saverwalter.Model
{
    public sealed class Heizkostenberechnung
    {
        public double Betrag;
        public double PauschalBetrag;

        public double tw;
        public double V;
        public double Q;

        public double Para7;
        public double Para8;

        public double GesamtNutzflaeche;
        public double NFZeitanteil;
        public double HeizkostenVerbrauchAnteil;
        public double WarmwasserVerbrauchAnteil;

        public double Para9_2;

        public double WaermeAnteilNF;
        public double WaermeAnteilVerb;
        public double WarmwasserAnteilNF;
        public double WarmwasserAnteilVerb;

        public double Kosten;

        public Heizkostenberechnung(
            Betriebskostenrechnung rechnung,
            Wohnung wohnung,
            DateOnly abrechnungsbeginn,
            DateOnly abrechnungsende,
            DateOnly nutzungsbeginn,
            DateOnly nutzungsende,
            double zeitanteil,
            List<Note> notes)
        {
            Betrag = rechnung.Betrag;
            PauschalBetrag = rechnung.Betrag * 1.05;

            tw = 60;
            Para7 = rechnung.Umlage.HKVO?.HKVO_P7 ?? 0.5; // HeizkostenV §7
            Para8 = rechnung.Umlage.HKVO?.HKVO_P8 ?? 0.5; // HeizkostenV §8

            var warmwasserZaehlerAbrechnungseinheit = rechnung.Umlage.Zaehler.Where(e => e.Typ == Zaehlertyp.Warmwasser).ToList();
            if (warmwasserZaehlerAbrechnungseinheit.Count == 0)
            {
                notes.Add(new Note("Keine Warmwasserzähler in Abrechnungseinheit gefunden.", Severity.Error));
            }

            var warmwasserZaehlerWohnung = warmwasserZaehlerAbrechnungseinheit.Where(z => z.Wohnung == wohnung).ToList();
            if (warmwasserZaehlerWohnung.Count == 0)
            {
                notes.Add(new Note("Keine Warmwasserzähler für Wohnung.", Severity.Error));
            }

            var allgemeinZaehler = rechnung.Umlage.Zaehler.Where(e => e.Wohnung is null).ToList();
            if (allgemeinZaehler.Count == 0)
            {
                new Note("Notwendiger Zähler für Umlage ist nicht definiert.", Severity.Error);
                return;
            }

            var gasZaehlerAbrechnungseinheit = rechnung.Umlage.Zaehler.Where(e => e.Typ == Zaehlertyp.Gas && e.Wohnung != null).ToList();
            var gasZaehlerWohnung = gasZaehlerAbrechnungseinheit.Where(e => e.Wohnung == wohnung).ToList();
            var gasZaehlerVerbrauchWohnung = Delta(gasZaehlerWohnung, nutzungsbeginn, nutzungsende);
            var gasAllgemeinZaehler = rechnung.Umlage.Zaehler.Where(z => z.Wohnung == null && z.Typ == Zaehlertyp.Gas).ToList();

            V = Delta(warmwasserZaehlerAbrechnungseinheit, abrechnungsbeginn, abrechnungsende);
            Q = Delta(gasAllgemeinZaehler, abrechnungsbeginn, abrechnungsende);

            if (Q == 0)
            {
                notes.Add(new Note("Gesamtzähler steht auf 0.", Severity.Error));
            }

            Para9_2 = 2.5 * (V / Q) * (tw - 10); // TODO HeizkostenV §9

            if (Para9_2 > 1)
            {
                notes.Add(new Note("Heizkostenverteilung nach §9 ist über 100%.", Severity.Error));
            }

            GesamtNutzflaeche = rechnung.Umlage.Wohnungen.Sum(w => w.Nutzflaeche);
            NFZeitanteil = wohnung.Nutzflaeche / GesamtNutzflaeche * zeitanteil;

            HeizkostenVerbrauchAnteil = Delta(gasZaehlerWohnung, nutzungsbeginn, nutzungsende) / Delta(gasZaehlerAbrechnungseinheit, nutzungsbeginn, nutzungsende);
            WarmwasserVerbrauchAnteil = Delta(warmwasserZaehlerWohnung, nutzungsbeginn, nutzungsende) / V;

            WaermeAnteilNF = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * NFZeitanteil;
            WaermeAnteilVerb = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * HeizkostenVerbrauchAnteil;
            WarmwasserAnteilNF = PauschalBetrag * Para9_2 * Para8 * NFZeitanteil;
            WarmwasserAnteilVerb = PauschalBetrag * Para9_2 * Para8 * WarmwasserVerbrauchAnteil;

            Kosten = WaermeAnteilNF + WaermeAnteilVerb + WarmwasserAnteilNF + WarmwasserAnteilVerb;
        }

        private static double Delta(IEnumerable<Zaehler> zaehlerList, DateOnly beginn, DateOnly ende)
        {
            var beginnValue = zaehlerList.Select(z => z.Staende.OrderBy(s => s.Datum)
                .LastOrDefault(l => l.Datum <= beginn && (beginn.DayNumber - l.Datum.DayNumber) < 30))
                .Where(zaehlerstand => zaehlerstand != null)
                .Select(zaehlerstand => zaehlerstand!)
                .Sum(w => w.Stand);

            var endeValue = zaehlerList.Select(z => z.Staende.OrderBy(s => s.Datum)
                .LastOrDefault(l => l.Datum <= ende && (ende.DayNumber - l.Datum.DayNumber) < 30))
                .Where(zaehlerstand => zaehlerstand != null)
                .Select(zaehlerstand => zaehlerstand!)
                .Sum(w => w.Stand);

            return endeValue - beginnValue;
        }
    }
}
