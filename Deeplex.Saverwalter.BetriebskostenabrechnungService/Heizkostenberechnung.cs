using System.Collections.Immutable;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Heizkostenberechnung
    {
        public double Betrag;
        public double PauschalBetrag;

        public double tw;
        public double V;
        //public double W;
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

        // TODO Zähler sind hier noch nicht so richtig drin. Aktuell werden einfach alle Zähler eines
        // Typen in einen Topf geworfen, aber Zähler referenzieren jetzt jeweils die Allgemeinzähler.
        // Man kann also auch direkt die Zähler des Allgemeinzählers der Rechnung nehmen
        public Heizkostenberechnung(
            Betriebskostenrechnung rechnung,
            List<Zaehler> alleZaehler,
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

            // TODO These should be variable
            tw = 60;
            Para7 = rechnung.Umlage.HKVO?.HKVO_P7 ?? 0.5; // HeizkostenV §7
            Para8 = rechnung.Umlage.HKVO?.HKVO_P8 ?? 0.5; // HeizkostenV §8

            // Alle Warmwasserzähler die in dieser Umlage betroffen sind
            var AlleWarmwasserZaehler = alleZaehler.Where(z =>
                z.Typ == Zaehlertyp.Warmwasser && rechnung.Umlage.Wohnungen.Contains(z.Wohnung!)).ToImmutableList();

            // Der Warmwasserzähler der Wohnung der Abrechnung
            var WarmwasserZaehler = AlleWarmwasserZaehler
                .Where(z => z.Wohnung == wohnung)
                .ToImmutableList();

            // TODO Fix this...
            if (rechnung.Umlage.HKVO == null)
            {
                new Note("Kein Zähler für die Umlage vorhanden", Severity.Error);
                //return;
            }
            var Allgemeinzaehler = rechnung.Umlage.HKVO?.Zaehler;

            if (Allgemeinzaehler == null)
            {
                new Note("Notwendiger Zähler für Umlage ist nicht definiert", Severity.Error);
                //return;
            }

            // Get all Zaehler for this Umlage for this Wohnung
            var WohnungWaermeZaehler = alleZaehler.Where(z =>
               z.Typ == Zaehlertyp.Gas && z.Wohnung == wohnung).ToList();
            //var WohnungWaermeZaehler = Allgemeinzaehler?.EinzelZaehler
            //    .Where(z => z.Wohnung == wohnung)
            //    .ToImmutableList();

            ImmutableList<Zaehlerstand> Ende(IEnumerable<Zaehler> z, bool ganzeGruppe = false)
            {
                var ende = ganzeGruppe ? abrechnungsende : nutzungsende;
                var ret = z.Select(z => z.Staende.OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= ende && (ende.DayNumber - l.Datum.DayNumber) < 30))
                    .Where(zaehlerstand => zaehlerstand != null)
                    .Select(zaehlerstand => zaehlerstand!)
                    .ToImmutableList();

                return ret;
            }

            ImmutableList<Zaehlerstand> Beginn(IEnumerable<Zaehler> zaehlerList, bool ganzeGruppe = false)
            {
                var beginn = (ganzeGruppe ? abrechnungsbeginn : nutzungsbeginn).AddDays(-1);
                var ret = zaehlerList.Select(z => z.Staende.OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= beginn && (beginn.DayNumber - l.Datum.DayNumber) < 30))
                    .Where(zaehlerstand => zaehlerstand != null)
                    .Select(zaehlerstand => zaehlerstand!)
                    .ToImmutableList();

                return ret;
            }

            V = Ende(AlleWarmwasserZaehler, true).Sum(zaehlerstand => zaehlerstand.Stand) -
                Beginn(AlleWarmwasserZaehler, true).Sum(zaehlerstand => zaehlerstand.Stand);

            //Q = Ende(new List<Zaehler>() { Allgemeinzaehler }, true).Sum(zaehlerstand => zaehlerstand.Stand) -
            //    Beginn(new List<Zaehler>() { Allgemeinzaehler }, true).Sum(zaehlerstand => zaehlerstand.Stand);
            
            var alleWaermeZaehler = alleZaehler.Where(z =>
                z.Typ == Zaehlertyp.Gas && rechnung.Umlage.Wohnungen.Contains(z.Wohnung!)).ToList();

            Q = Ende(alleWaermeZaehler, true).Sum(zaehlerstand => zaehlerstand.Stand) -
                Beginn(alleWaermeZaehler, true).Sum(zaehlerstand => zaehlerstand.Stand);


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

            HeizkostenVerbrauchAnteil = (Ende(WohnungWaermeZaehler).Sum(w => w.Stand) - Beginn(WohnungWaermeZaehler).Sum(w => w.Stand)) / Q;
            WarmwasserVerbrauchAnteil = (Ende(WarmwasserZaehler).Sum(w => w.Stand) - Beginn(WarmwasserZaehler).Sum(w => w.Stand)) / V;

            WaermeAnteilNF = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * NFZeitanteil;
            WaermeAnteilVerb = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * HeizkostenVerbrauchAnteil;
            WarmwasserAnteilNF = PauschalBetrag * Para9_2 * Para8 * NFZeitanteil;
            WarmwasserAnteilVerb = PauschalBetrag * Para9_2 * Para8 * WarmwasserVerbrauchAnteil;

            Kosten = WaermeAnteilNF + WaermeAnteilVerb + WarmwasserAnteilNF + WarmwasserAnteilVerb;
        }
    }
}
