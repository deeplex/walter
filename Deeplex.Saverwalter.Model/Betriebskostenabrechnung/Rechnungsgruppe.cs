using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Deeplex.Saverwalter.Model.Utils;

namespace Deeplex.Saverwalter.Model
{

    public sealed class Rechnungsgruppe
    {
        public List<Betriebskostenrechnung> Rechnungen;

        public double GesamtWohnflaeche;
        public double WFZeitanteil;
        public double NFZeitanteil;
        public double GesamtNutzflaeche;
        public int GesamtEinheiten;
        public double NEZeitanteil;
        public List<(DateTime Beginn, DateTime Ende, int Personenzahl)> GesamtPersonenIntervall;
        public List<(DateTime Beginn, DateTime Ende, int Personenzahl)> PersonenIntervall;
        public List<(DateTime Beginn, DateTime Ende, double Anteil)> PersZeitanteil;
        public Dictionary<Betriebskostentyp, List<(string Kennnummer, Zaehlertyp Typ, double Delta, double Anteil)>> Verbrauch;
        public Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch;
        public Dictionary<Betriebskostentyp, double> VerbrauchAnteil;
        public List<Heizkostenberechnung> Heizkosten;
        public double GesamtBetragKalt;
        public double GesamtBetragWarm;
        public double BetragKalt;
        public double BetragWarm;

        public Rechnungsgruppe(Betriebskostenabrechnung b, List<Betriebskostenrechnung> gruppe)
        {
            var gr = gruppe.First().Gruppen;

            Rechnungen = gruppe;
            GesamtWohnflaeche = gr.Sum(w => w.Wohnung.Wohnflaeche);
            GesamtNutzflaeche = gr.Sum(w => w.Wohnung.Nutzflaeche);
            GesamtEinheiten = gr.Sum(w => w.Wohnung.Nutzeinheit);

            var alleVertraegeDieserWohnungen = gr.SelectMany(w => w.Wohnung.Vertraege.Where(v =>
                v.Beginn <= b.Abrechnungsende && (v.Ende is null || v.Ende >= b.Abrechnungsbeginn)));

            GesamtPersonenIntervall =
                VertraegeIntervallPersonenzahl(alleVertraegeDieserWohnungen,
                b.Abrechnungsbeginn, b.Abrechnungsende).ToList();

            PersonenIntervall =
                VertraegeIntervallPersonenzahl(b.Vertragsversionen,
                b.Nutzungsbeginn, b.Nutzungsende).ToList();

            WFZeitanteil = b.Wohnung.Wohnflaeche / GesamtWohnflaeche * b.Zeitanteil;
            NFZeitanteil = b.Wohnung.Nutzflaeche / GesamtNutzflaeche * b.Zeitanteil;
            NEZeitanteil = (double)b.Wohnung.Nutzeinheit / GesamtEinheiten * b.Zeitanteil;

            PersZeitanteil = GesamtPersonenIntervall
                .Where(g => g.Beginn < b.Nutzungsende && g.Ende >= b.Nutzungsbeginn)
                .Select((w, i) =>
                    (w.Beginn, w.Ende, Anteil:
                        (double)PersonenIntervall.Where(p => p.Beginn <= w.Beginn).First().Personenzahl / w.Personenzahl *
                        (((double)(w.Ende - w.Beginn).Days + 1) / b.Abrechnungszeitspanne))).ToList();

            GesamtVerbrauch = gruppe
                .Where(g => g.Schluessel == UmlageSchluessel.NachVerbrauch)
                .Select(r => b.GetVerbrauch(r, true))
                .ToDictionary(g => g.First().bTyp, g => g.GroupBy(gg => gg.zTyp)
                .Select(gg => (gg.Key, gg.Sum(ggg => ggg.Delta))).ToList());
            Verbrauch = gruppe
                .Where(g => g.Schluessel == UmlageSchluessel.NachVerbrauch)
                .Select(r => b.GetVerbrauch(r))
                .ToDictionary(r => r.First().bTyp, r => r
                    .Select(rr => (rr.Kennnummer, rr.zTyp, rr.Delta, rr.Delta / GesamtVerbrauch[r.First().bTyp]
                        .First(rrr => rrr.Typ == rr.zTyp).Delta))
                        .ToList());

            VerbrauchAnteil = Verbrauch
                .Select(v => (v.Key, v.Value.Sum(vv => vv.Delta), v.Value.Sum(vv => vv.Delta / vv.Anteil)))
                .ToDictionary(v => v.Key, v => v.Item2 / v.Item3);

            var gruppeKalt = gruppe.Where(r => (int)r.Typ % 2 == 0);
            GesamtBetragKalt = gruppeKalt.Sum(r => r.Betrag);
            BetragKalt = gruppeKalt.Aggregate(0.0, (a, r) =>
                r.Schluessel switch
                {
                    UmlageSchluessel.NachWohnflaeche => a + r.Betrag * WFZeitanteil,
                    UmlageSchluessel.NachNutzeinheit => a + r.Betrag * NEZeitanteil,
                    UmlageSchluessel.NachPersonenzahl => a + PersZeitanteil.Aggregate(0.0, (a2, z) => a2 += z.Anteil * r.Betrag),
                    UmlageSchluessel.NachVerbrauch => a + r.Betrag * VerbrauchAnteil[r.Typ],
                    _ => a + 0, // TODO or throw something...
                    }
            );

            Heizkosten = gruppe
                .Where(r => (int)r.Typ % 2 == 1)
                .Select(r => new Heizkostenberechnung(r, b))
                .ToList();
            GesamtBetragWarm = Heizkosten.Sum(h => h.PauschalBetrag);
            BetragWarm = Heizkosten.Sum(h => h.Kosten);

            for (int i = 0, count = GesamtPersonenIntervall.Count - 1; i < count; ++i)
            {
                if (GesamtPersonenIntervall[i].Personenzahl == GesamtPersonenIntervall[i + 1].Personenzahl)
                {
                    var Beginn = GesamtPersonenIntervall[i].Beginn;
                    var Ende = GesamtPersonenIntervall[i + 1].Ende;

                    GesamtPersonenIntervall[i] = (Beginn, Ende, GesamtPersonenIntervall[i].Personenzahl);
                    GesamtPersonenIntervall.RemoveAt(1 + i--);
                    count--;
                }
            }

            for (int i = 0, count = PersZeitanteil.Count - 1; i < count; ++i)
            {
                var gpz = GesamtPersonenIntervall.Last(g => g.Beginn.Date <= PersZeitanteil[i].Beginn.Date).Personenzahl;
                var ngpz = GesamtPersonenIntervall.Last(g => g.Beginn.Date <= PersZeitanteil[i + 1].Beginn.Date).Personenzahl;
                var pz = PersonenIntervall.Last(p => p.Beginn.Date <= PersZeitanteil[i].Beginn).Personenzahl;
                var npz = PersonenIntervall.Last(p => p.Beginn.Date <= PersZeitanteil[i + 1].Beginn).Personenzahl;

                if (gpz == ngpz && pz == npz)
                {
                    PersZeitanteil[i] = (
                        PersZeitanteil[i].Beginn,
                        PersZeitanteil[i + 1].Ende,
                        PersZeitanteil[i].Anteil + PersZeitanteil[i + 1].Anteil);

                    PersZeitanteil.RemoveAt(1 + i--);
                    count--;
                }
            }
        }

        private static List<(DateTime Beginn, DateTime Ende, int Personenzahl)>
            VertraegeIntervallPersonenzahl(IEnumerable<Vertrag> vertraege, DateTime Beginn, DateTime Ende)
        {
            var merged = vertraege
                .Where(v => v.Beginn <= Ende && (v.Ende is null || v.Ende >= Beginn))
                .SelectMany(v => new[]
                {
                    (Max(v.Beginn, Beginn), v.Personenzahl),
                    (Min(v.Ende ?? Ende, Ende).AddDays(1), -v.Personenzahl)
                })
                .GroupBy(t => t.Item1.Date)
                .Select(g => (Beginn: g.Key, Ende, Personenzahl: g.Sum(t => t.Item2)))
                .OrderBy(t => t.Beginn)
                .ToList();

            for (int i = 0; i < merged.Count; ++i)
            {
                merged[i] = (
                    merged[i].Beginn,
                    i + 1 < merged.Count ? merged[i + 1].Beginn.AddDays(-1) : Ende,
                    i - 1 >= 0 ? merged[i - 1].Personenzahl + merged[i].Personenzahl : merged[i].Personenzahl);
            }
            merged.RemoveAt(merged.Count - 1);

            return merged;
        }
    }

}
