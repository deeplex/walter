using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class Heizkostenberechnung
    {
        public double GesamtBetrag { get; }
        public double PauschalBetrag { get; }

        public double tw { get; }
        public double V { get; }
        public double Q { get; }

        public double Para7 { get; }
        public double Para8 { get; }

        public double GesamtNutzflaeche { get; }
        public double NFZeitanteil { get; }
        public double HeizkostenVerbrauchAnteil { get; }
        public double WarmwasserVerbrauchAnteil { get; }

        public double Para9_2 { get; }

        public double WaermeAnteilNF { get; }
        public double WaermeAnteilVerb { get; }
        public double WarmwasserAnteilNF { get; }
        public double WarmwasserAnteilVerb { get; }

        public double Betrag { get; }

        public Heizkostenberechnung(
            Betriebskostenrechnung rechnung,
            Wohnung wohnung,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            GesamtBetrag = rechnung.Betrag;
            PauschalBetrag = rechnung.Betrag * 1.05;

            tw = 60;
            Para7 = rechnung.Umlage.HKVO?.HKVO_P7 ?? 0.5; // HeizkostenV §7
            Para8 = rechnung.Umlage.HKVO?.HKVO_P8 ?? 0.5; // HeizkostenV §8

            var warmwasserZaehlerAbrechnungseinheit = rechnung.Umlage.Zaehler.Where(e => e.Typ == Zaehlertyp.Warmwasser).ToList();
            if (warmwasserZaehlerAbrechnungseinheit.Count == 0)
            {
                notes.Add("Keine Warmwasserzähler in Abrechnungseinheit gefunden.", Severity.Error);
            }

            var warmwasserZaehlerWohnung = warmwasserZaehlerAbrechnungseinheit.Where(z => z.Wohnung == wohnung).ToList();
            if (warmwasserZaehlerWohnung.Count == 0)
            {
                notes.Add("Keine Warmwasserzähler für Wohnung.", Severity.Error);
            }

            var gasZaehlerAbrechnungseinheit = rechnung.Umlage.Zaehler.Where(e => e.Typ == Zaehlertyp.Gas && e.Wohnung != null).ToList();
            var gasZaehlerWohnung = gasZaehlerAbrechnungseinheit.Where(e => e.Wohnung == wohnung).ToList();
            var gasZaehlerVerbrauchWohnung = Delta(gasZaehlerWohnung, zeitraum.Nutzungsbeginn, zeitraum.Nutzungsende);
            var gasAllgemeinZaehler = rechnung.Umlage.Zaehler.Where(z => z.Wohnung == null && z.Typ == Zaehlertyp.Gas).ToList();

            if (gasAllgemeinZaehler.Count == 0)
            {
                notes.Add($"Notwendiger Allgemeinzähler Gas für Heizkosten ist nicht definiert.", Severity.Error);
                return;
            }


            V = Delta(warmwasserZaehlerAbrechnungseinheit, zeitraum.Abrechnungsbeginn, zeitraum.Abrechnungsende);
            Q = Delta(gasAllgemeinZaehler, zeitraum.Abrechnungsbeginn, zeitraum.Abrechnungsende);

            if (Q == 0)
            {
                notes.Add("Gesamtzähler steht auf 0.", Severity.Error);
            }

            Para9_2 = 2.5 * (V / Q) * (tw - 10); // TODO HeizkostenV §9

            if (Para9_2 > 1)
            {
                notes.Add("Heizkostenverteilung nach §9 ist über 100%.", Severity.Error);
            }

            GesamtNutzflaeche = rechnung.Umlage.Wohnungen.Sum(w => w.Nutzflaeche);
            NFZeitanteil = wohnung.Nutzflaeche / GesamtNutzflaeche * zeitraum.Zeitanteil;

            HeizkostenVerbrauchAnteil = Delta(gasZaehlerWohnung, zeitraum.Nutzungsbeginn, zeitraum.Nutzungsende) /
                Delta(gasZaehlerAbrechnungseinheit, zeitraum.Nutzungsbeginn, zeitraum.Nutzungsende);
            WarmwasserVerbrauchAnteil = Delta(warmwasserZaehlerWohnung, zeitraum.Nutzungsbeginn, zeitraum.Nutzungsende) / V;

            WaermeAnteilNF = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * NFZeitanteil;
            WaermeAnteilVerb = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * HeizkostenVerbrauchAnteil;
            WarmwasserAnteilNF = PauschalBetrag * Para9_2 * Para8 * NFZeitanteil;
            WarmwasserAnteilVerb = PauschalBetrag * Para9_2 * Para8 * WarmwasserVerbrauchAnteil;

            Betrag = WaermeAnteilNF + WaermeAnteilVerb + WarmwasserAnteilNF + WarmwasserAnteilVerb;
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
