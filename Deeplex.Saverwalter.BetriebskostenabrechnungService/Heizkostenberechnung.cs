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
        public Heizkostenberechnung(SaverwalterContext ctx, Betriebskostenrechnung r, BetriebskostenabrechnungService.IBetriebskostenabrechnung b)
        {
            Betrag = r.Betrag;
            PauschalBetrag = r.Betrag * 1.05;

            // TODO These should be variable
            tw = 60;
            Para7 = r.Umlage.HKVO?.HKVO_P7 ?? 0.5; // HeizkostenV §7
            Para8 = r.Umlage.HKVO?.HKVO_P8 ?? 0.5; // HeizkostenV §8

            // Alle Warmwasserzähler die in dieser Umlage betroffen sind
            var AlleWarmwasserZaehler = ctx.ZaehlerSet.Where(z =>
                z.Typ == Zaehlertyp.Warmwasser && r.Umlage.Wohnungen.Contains(z.Wohnung!)).ToImmutableList();

            // Der Warmwasserzähler der Wohnung der Abrechnung
            var WarmwasserZaehler = AlleWarmwasserZaehler
                .Where(z => z.Wohnung == b.Wohnung)
                .ToImmutableList();

            if (r.Umlage.HKVO == null)
            {
                new Note("Kein Zähler für die Umlage vorhanden", Severity.Error);
                return;
            }
            var Allgemeinzaehler = r.Umlage.HKVO!.Zaehler;

            if (Allgemeinzaehler == null)
            {
                new Note("Notwendiger Zähler für Umlage ist nicht definiert", Severity.Error);
                return;
            }

            // Get all Zaehler for this Umlage for this Wohnung
            var WohnungWaermeZaehler = Allgemeinzaehler!.EinzelZaehler
                .Where(z => z.Wohnung == b.Wohnung)
                .ToImmutableList();

            ImmutableList<Zaehlerstand> Ende(IEnumerable<Zaehler> z, bool ganzeGruppe = false)
            {
                var ende = ganzeGruppe ? b.Abrechnungsende : b.Nutzungsende;
                var ret = z.Select(z => z.Staende.OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= ende && (ende.DayNumber - l.Datum.DayNumber) < 30))
                    .Where(zaehlerstand => zaehlerstand != null)
                    .Select(zaehlerstand => zaehlerstand!)
                    .ToImmutableList();

                return ret;
            }

            ImmutableList<Zaehlerstand> Beginn(IEnumerable<Zaehler> zaehlerList, bool ganzeGruppe = false)
            {
                var beginn = (ganzeGruppe ? b.Abrechnungsbeginn : b.Nutzungsbeginn).AddDays(-1);
                var ret = zaehlerList.Select(z => z.Staende.OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= beginn && (beginn.DayNumber - l.Datum.DayNumber) < 30))
                    .Where(zaehlerstand => zaehlerstand != null)
                    .Select(zaehlerstand => zaehlerstand!)
                    .ToImmutableList();

                return ret;
            }

            V = Ende(AlleWarmwasserZaehler, true).Sum(zaehlerstand => zaehlerstand.Stand) -
                Beginn(AlleWarmwasserZaehler, true).Sum(zaehlerstand => zaehlerstand.Stand);

            Q = Ende(new List<Zaehler>() { Allgemeinzaehler }, true).Sum(zaehlerstand => zaehlerstand.Stand) -
                Beginn(new List<Zaehler>() { Allgemeinzaehler }, true).Sum(zaehlerstand => zaehlerstand.Stand);

            if (Q == 0)
            {
                b.Notes.Add(new Note("Gesamtzähler steht auf 0.", Severity.Error));
            }

            Para9_2 = 2.5 * (V / Q) * (tw - 10); // TODO HeizkostenV §9

            if (Para9_2 > 1)
            {
                b.Notes.Add(new Note("Heizkostenverteilung nach §9 ist über 100%.", Severity.Error));
            }

            GesamtNutzflaeche = r.Umlage.Wohnungen.Sum(w => w.Nutzflaeche);
            NFZeitanteil = b.Wohnung.Nutzflaeche / GesamtNutzflaeche * b.Zeitanteil;

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
