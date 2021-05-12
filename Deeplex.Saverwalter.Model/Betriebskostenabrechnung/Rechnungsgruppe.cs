using System;
using System.Collections.Generic;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;

namespace Deeplex.Saverwalter.Model
{

    public sealed class Rechnungsgruppe
    {
        public List<Betriebskostenrechnung> Rechnungen;

        private Betriebskostenabrechnung b { get; }
        private List<BetriebskostenrechnungsGruppe> gr => Rechnungen.First().Gruppen;
        private IEnumerable<Vertrag> alleVertraegeDieserWohnungen => gr.SelectMany(w => w.Wohnung.Vertraege.Where(v =>
                v.Beginn <= b.Abrechnungsende && (v.Ende is null || v.Ende >= b.Abrechnungsbeginn)));


        public double GesamtWohnflaeche => gr.Sum(w => w.Wohnung.Wohnflaeche);
        public double WFZeitanteil => b.Wohnung.Wohnflaeche / GesamtWohnflaeche * b.Zeitanteil;
        public double NFZeitanteil => b.Wohnung.Nutzflaeche / GesamtNutzflaeche * b.Zeitanteil;
        public double GesamtNutzflaeche => gr.Sum(w => w.Wohnung.Nutzflaeche);
        public int GesamtEinheiten => gr.Sum(w => w.Wohnung.Nutzeinheit);
        public double NEZeitanteil => (double)b.Wohnung.Nutzeinheit / GesamtEinheiten * b.Zeitanteil;
        public List<(DateTime Beginn, DateTime Ende, int Personenzahl)> GesamtPersonenIntervall =>
            VertraegeIntervallPersonenzahl(alleVertraegeDieserWohnungen, b.Abrechnungsbeginn, b.Abrechnungsende).ToList();

        public List<(DateTime Beginn, DateTime Ende, int Personenzahl)> PersonenIntervall =>
            VertraegeIntervallPersonenzahl(b.Vertragsversionen, b.Nutzungsbeginn, b.Nutzungsende).ToList();

        public List<(DateTime Beginn, DateTime Ende, double Anteil)> PersZeitanteil => GesamtPersonenIntervall
                .Where(g => g.Beginn < b.Nutzungsende && g.Ende >= b.Nutzungsbeginn)
                .Select((w, i) =>
                    (w.Beginn, w.Ende, Anteil:
                        (double)PersonenIntervall.Where(p => p.Beginn <= w.Beginn).First().Personenzahl / w.Personenzahl *
                        (((double)(w.Ende - w.Beginn).Days + 1) / b.Abrechnungszeitspanne))).ToList();

        public Dictionary<Betriebskostentyp, List<(string Kennnummer, Zaehlertyp Typ, double Delta, double Anteil)>> Verbrauch => Rechnungen
            .Where(g => g.Schluessel == UmlageSchluessel.NachVerbrauch)
            .Select(r => b.GetVerbrauch(r))
            .ToDictionary(r => r.First().bTyp, r => r
                .Select(rr => (rr.Kennnummer, rr.zTyp, rr.Delta, rr.Delta / GesamtVerbrauch[r.First().bTyp]
                    .First(rrr => rrr.Typ == rr.zTyp).Delta))
                    .ToList());

        public Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch => Rechnungen
            .Where(g => g.Schluessel == UmlageSchluessel.NachVerbrauch)
            .Select(r => b.GetVerbrauch(r, true))
            .ToDictionary(g => g.First().bTyp, g => g.GroupBy(gg => gg.zTyp)
            .Select(gg => (gg.Key, gg.Sum(ggg => ggg.Delta))).ToList());

        public Dictionary<Betriebskostentyp, double> VerbrauchAnteil => Verbrauch
            .Select(v => (v.Key, v.Value.Sum(vv => vv.Delta), v.Value.Sum(vv => vv.Delta / vv.Anteil)))
            .ToDictionary(v => v.Key, v => v.Item2 / v.Item3);
        public List<Heizkostenberechnung> Heizkosten => Rechnungen
            .Where(r => (int)r.Typ % 2 == 1).Select(r => new Heizkostenberechnung(r, b)).ToList();
        public double GesamtBetragKalt => Rechnungen.Where(r => (int)r.Typ % 2 == 0).Sum(r => r.Betrag);
        public double BetragKalt => Rechnungen.Where(r => (int)r.Typ % 2 == 0).Aggregate(0.0, (a, r) =>
            r.Schluessel switch
            {
                UmlageSchluessel.NachWohnflaeche => a + r.Betrag * WFZeitanteil,
                UmlageSchluessel.NachNutzeinheit => a + r.Betrag * NEZeitanteil,
                UmlageSchluessel.NachPersonenzahl => a + PersZeitanteil.Aggregate(0.0, (a2, z) => a2 += z.Anteil * r.Betrag),
                UmlageSchluessel.NachVerbrauch => a + r.Betrag * VerbrauchAnteil[r.Typ],
                _ => a + 0, // TODO or throw something...
            });
        public double GesamtBetragWarm => Heizkosten.Sum(h => h.PauschalBetrag);
        public double BetragWarm => Heizkosten.Sum(h => h.Kosten);


        public Rechnungsgruppe(Betriebskostenabrechnung _b, List<Betriebskostenrechnung> gruppe)
        {
            Rechnungen = gruppe;
            b = _b;
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
