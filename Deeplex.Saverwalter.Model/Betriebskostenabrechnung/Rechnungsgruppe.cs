using System;
using System.Collections.Generic;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;

namespace Deeplex.Saverwalter.Model
{
    public interface IRechnungsgruppe
    {
        string Bezeichnung { get; }
        List<PersonenZeitIntervall> PersonenIntervall { get; }
        List<PersonenZeitIntervall> GesamtPersonenIntervall { get; }
        double GesamtWohnflaeche { get; }
        double GesamtNutzflaeche { get; }
        int GesamtEinheiten { get; }
        double WFZeitanteil { get; }
        double NFZeitanteil { get; }
        double NEZeitanteil { get; }
        List<Umlage> Umlagen { get; }
        List<(DateTime Beginn, DateTime Ende, double Anteil)> PersZeitanteil { get; }
        Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch { get; }
        Dictionary<Betriebskostentyp, double> VerbrauchAnteil { get; }
        double BetragKalt { get; }
        List<Heizkostenberechnung> Heizkosten { get; }
        double BetragWarm { get; }
        double GesamtBetragWarm { get; }
    }

    public sealed class Rechnungsgruppe : IRechnungsgruppe
    {
        public List<Umlage> Umlagen { get; }

        private IBetriebskostenabrechnung b { get; }
        private List<Wohnung> gr => Umlagen.First().Wohnungen.ToList();
        private IEnumerable<Vertrag> alleVertraegeDieserWohnungen => gr.SelectMany(w => w.Vertraege.Where(v =>
                v.Beginn <= b.Abrechnungsende && (v.Ende is null || v.Ende >= b.Abrechnungsbeginn)));

        public string Bezeichnung => Umlagen.First().GetWohnungenBezeichnung();
        public double GesamtWohnflaeche => gr.Sum(w => w.Wohnflaeche);
        public double WFZeitanteil => b.Wohnung.Wohnflaeche / GesamtWohnflaeche * b.Zeitanteil;
        public double NFZeitanteil => b.Wohnung.Nutzflaeche / GesamtNutzflaeche * b.Zeitanteil;
        public double GesamtNutzflaeche => gr.Sum(w => w.Nutzflaeche);
        public int GesamtEinheiten => gr.Sum(w => w.Nutzeinheit);
        public double NEZeitanteil => (double)b.Wohnung.Nutzeinheit / GesamtEinheiten * b.Zeitanteil;
        public List<PersonenZeitIntervall> GesamtPersonenIntervall
        {
            get
            {
                var self = this;
                return VertraegeIntervallPersonenzahl(alleVertraegeDieserWohnungen, b.Abrechnungsbeginn, b.Abrechnungsende, self).ToList();
            }
        }

        public List<PersonenZeitIntervall> PersonenIntervall
        {
            get
            {
                var self = this;
                return VertraegeIntervallPersonenzahl(b.Vertragsversionen, b.Nutzungsbeginn, b.Nutzungsende, self).ToList();
            }
        }

        public List<(DateTime Beginn, DateTime Ende, double Anteil)> PersZeitanteil => GesamtPersonenIntervall
                .Where(g => g.Beginn < b.Nutzungsende && g.Ende >= b.Nutzungsbeginn)
                    .Select((w, i) =>
                        (w.Beginn, w.Ende, Anteil:
                            (double)(PersonenIntervall.FirstOrDefault(p => p.Beginn <= w.Beginn)?.Personenzahl ?? 0) / w.Personenzahl *
                            (((double)(w.Ende - w.Beginn).Days + 1) / b.Abrechnungszeitspanne))).ToList();

        public Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch
        {
            get
            {
                var VerbrauchList = Umlagen
                    .Where(g => g.Schluessel == Umlageschluessel.NachVerbrauch)
                    .SelectMany(u => u.Betriebskostenrechnungen)
                    .Where(r => r.BetreffendesJahr == b.Jahr)
                    .Select(r => b.GetVerbrauch(r));

                if (VerbrauchList.Any(w => w.Count() == 0))
                {
                    // TODO this can be made even more explicit.
                    b.notes.Add(new Note("Für eine Rechnung konnte keine Zuordnung erstellt werden.", Severity.Error));
                    return new Dictionary<Betriebskostentyp, List<VerbrauchAnteil>>();
                }

                return VerbrauchList.ToDictionary(r =>
                    r.First().Betriebskostentyp,
                    r => r.Select(rr => new VerbrauchAnteil(rr.Kennnummer, rr.Zaehlertyp, rr.Delta, rr.Delta / GesamtVerbrauch[r.First().Betriebskostentyp]
                        .First(rrr => rrr.Typ == rr.Zaehlertyp).Delta))
                        .ToList());
            }
        }


        public Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch => Umlagen
            .Where(g => g.Schluessel == Umlageschluessel.NachVerbrauch)
            .SelectMany(u => u.Betriebskostenrechnungen)
            .Where(u => u.BetreffendesJahr == b.Jahr)
            .Select(r => b.GetVerbrauch(r, true))
        .ToDictionary(g => g.First().Betriebskostentyp, g => g.GroupBy(gg => gg.Zaehlertyp)
        .Select(gg => (gg.Key, gg.Sum(ggg => ggg.Delta))).ToList());

        private double checkVerbrauch(Betriebskostentyp t)
        {
            if (VerbrauchAnteil.ContainsKey(t))
            {
                return VerbrauchAnteil[t];
            }
            else
            {
                b.notes.Add(new Note("Konnte keinen Anteil für " + t.ToDescriptionString() + " feststellen.", Severity.Error));
                return 0;
            }
        }

        public Dictionary<Betriebskostentyp, double> VerbrauchAnteil => Verbrauch
            .Select(v => (v.Key, v.Value.Sum(vv => vv.Delta), v.Value.Sum(vv => vv.Delta / vv.Anteil)))
            .ToDictionary(v => v.Key, v => v.Item2 / v.Item3);
        public List<Heizkostenberechnung> Heizkosten => Umlagen
            .Where(r => (int)r.Typ % 2 == 1)
            .SelectMany(u => u.Betriebskostenrechnungen)
            .Where(r => r.BetreffendesJahr == b.Jahr)
            .Select(r => new Heizkostenberechnung(r, b))
            .ToList();
        public double GesamtBetragKalt => Umlagen
                .Where(r => (int)r.Typ % 2 == 0)
                .SelectMany(u => u.Betriebskostenrechnungen)
                .Where(r => r.BetreffendesJahr == b.Jahr)
                .Sum(r => r.Betrag);
        public double BetragKalt => Umlagen
            .Where(r => (int)r.Typ % 2 == 0)
            .SelectMany(u => u.Betriebskostenrechnungen)
            .Where(r => r.BetreffendesJahr == b.Jahr)
            .Aggregate(0.0, (a, r) =>
                r.Umlage.Schluessel switch
                {
                    Umlageschluessel.NachWohnflaeche => a + r.Betrag * WFZeitanteil,
                    Umlageschluessel.NachNutzeinheit => a + r.Betrag * NEZeitanteil,
                    Umlageschluessel.NachPersonenzahl => a + PersZeitanteil.Aggregate(0.0, (a2, z) => a2 += z.Anteil * r.Betrag),
                    Umlageschluessel.NachVerbrauch => a + r.Betrag * checkVerbrauch(r.Umlage.Typ),
                    _ => a + 0, // TODO or throw something...
                });
        public double GesamtBetragWarm => Heizkosten.Sum(h => h.PauschalBetrag);
        public double BetragWarm => Heizkosten.Sum(h => h.Kosten);

        public Rechnungsgruppe(IBetriebskostenabrechnung _b, List<Umlage> gruppe)
        {
            Umlagen = gruppe;
            b = _b;

            // TODO!
            // Remove 5% of Heizkosten to allgStrom (they are added there)
            // TODO only the first one is used here...
            //var allgStrom = Umlagen
            //    .Find(r => r.Typ == Betriebskostentyp.AllgemeinstromHausbeleuchtung)
            //    .Betriebskostenrechnungen
            //    .Where(r => r.BetreffendesJahr == b.Jahr)
            //    .First();
            // TODO this 0.05 has to be variable.
            //if (allgStrom != null && !b.AllgStromVerrechnetMitHeizkosten)
            //{
            //    var copy = allgStrom.ShallowCopy();

            //    copy.Betrag -= b.Vertrag.Wohnung.Umlagen.SelectMany(u => u.Betriebskostenrechnungen)
            //        .Where(g => g.BetreffendesJahr == b.Jahr && (int)g.Umlage.Typ % 2 == 1)
            //        .Select(r => r.Betrag)
            //        .Sum() * 0.05;
            //    b.AllgStromVerrechnetMitHeizkosten = true;
            //    if (copy.Betrag < 0)
            //    {
            //        b.notes.Add(new Note("Allgemeinstrom hat einen Betrag von " + string.Format("{0:N2}€", copy.Betrag), Severity.Error));
            //    }
            //}
        }

        private static List<PersonenZeitIntervall>
            VertraegeIntervallPersonenzahl(IEnumerable<Vertrag> vertraege, DateTime Beginn, DateTime Ende, Rechnungsgruppe parent)
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

            // TODO refactor function to switch from tuple to class - or replace this function by constructor
            var ret = merged.Select(m => new PersonenZeitIntervall(m, parent)).ToList();

            return ret;
        }
    }
}
