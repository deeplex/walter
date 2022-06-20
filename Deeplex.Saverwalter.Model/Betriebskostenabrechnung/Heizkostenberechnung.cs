using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

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
        public Heizkostenberechnung(Betriebskostenrechnung r, IBetriebskostenabrechnung b)
        {
            Betrag = r.Betrag;
            PauschalBetrag = r.Betrag * 1.05;

            // TODO These should be variable
            tw = 60;
            Para7 = r.HKVO_P7 ?? 0.5; // HeizkostenV §7
            Para8 = r.HKVO_P8 ?? 0.5; // HeizkostenV §8

            var AllgWarmwasserZaehler = b.db.ZaehlerSet.Where(z =>
                z.Typ == Zaehlertyp.Warmwasser && r.Wohnungen.Contains(z.Wohnung!)).ToImmutableList(); // TODO15 !

            var AllgWaermeZaehler = b.db.ZaehlerSet.Where(z =>
                z.Typ == Zaehlertyp.Gas && r.Wohnungen.Contains(z.Wohnung!)).ToImmutableList(); // TODO15 !

            var WaermeZaehler = AllgWaermeZaehler
                .Where(z => z.Wohnung == b.Wohnung)
                .ToImmutableList();

            var WarmwasserZaehler = AllgWarmwasserZaehler
                .Where(z => z.Wohnung == b.Wohnung)
                .ToImmutableList();

            ImmutableList<Zaehlerstand> Ende(ImmutableList<Zaehler> z, bool ganzeGruppe = false)
            {
                var ende = (ganzeGruppe ? b.Abrechnungsende : b.Nutzungsende).Date;
                var ret = z.Select(z => z.Staende.OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum.Date <= ende && (ende - l.Datum.Date).Days < 30))
                    .Where(zs => zs != null)
                    .ToImmutableList();

                return ret;
            }

            ImmutableList<Zaehlerstand> Beginn(ImmutableList<Zaehler> z, bool ganzeGruppe = false)
            {
                var beginn = (ganzeGruppe ? b.Abrechnungsbeginn : b.Nutzungsbeginn).Date.AddDays(-1);
                var ret = z.Select(z => z.Staende.OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum.Date <= beginn && (beginn - l.Datum.Date).Days < 30))
                    .Where(zs => zs != null)
                    .ToImmutableList();

                return ret;
            }

            V = Ende(AllgWarmwasserZaehler, true).Sum(w => w.Stand) -
                Beginn(AllgWarmwasserZaehler, true).Sum(w => w.Stand);

            Q = Ende(AllgWaermeZaehler, true).Sum(w => w.Stand) -
                Beginn(AllgWaermeZaehler, true).Sum(w => w.Stand);

            if (Q == 0)
            {
                b.notes.Add(new Note("Gesamtzähler steht auf 0.", Severity.Error));
            }

            Para9_2 = 2.5 * (V / Q) * (tw - 10); // TODO HeizkostenV §9

            if (Para9_2 > 1)
            {
                b.notes.Add(new Note("Heizkostenverteilung nach §9 ist über 100%.", Severity.Error));
            }

            GesamtNutzflaeche = r.Wohnungen.Sum(w => w.Nutzflaeche);
            NFZeitanteil = b.Wohnung.Nutzflaeche / GesamtNutzflaeche * b.Zeitanteil;

            HeizkostenVerbrauchAnteil = (Ende(WaermeZaehler).Sum(w => w.Stand) - Beginn(WaermeZaehler).Sum(w => w.Stand)) / Q;
            WarmwasserVerbrauchAnteil = (Ende(WarmwasserZaehler).Sum(w => w.Stand) - Beginn(WarmwasserZaehler).Sum(w => w.Stand)) / V;

            WaermeAnteilNF = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * NFZeitanteil;
            WaermeAnteilVerb = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * HeizkostenVerbrauchAnteil;
            WarmwasserAnteilNF = PauschalBetrag * Para9_2 * Para8 * NFZeitanteil;
            WarmwasserAnteilVerb = PauschalBetrag * Para9_2 * Para8 * WarmwasserVerbrauchAnteil;

            Kosten = WaermeAnteilNF + WaermeAnteilVerb + WarmwasserAnteilNF + WarmwasserAnteilVerb;
        }
    }
}
