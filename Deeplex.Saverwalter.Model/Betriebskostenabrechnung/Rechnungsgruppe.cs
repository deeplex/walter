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

        public string Bezeichnung => Rechnungen.First().GetWohnungenBezeichnung();
        public double GesamtWohnflaeche => gr.Sum(w => w.Wohnung.Wohnflaeche);
        public double WFZeitanteil => b.Wohnung.Wohnflaeche / GesamtWohnflaeche * b.Zeitanteil;
        public double NFZeitanteil => b.Wohnung.Nutzflaeche / GesamtNutzflaeche * b.Zeitanteil;
        public double GesamtNutzflaeche => gr.Sum(w => w.Wohnung.Nutzflaeche);
        public int GesamtEinheiten => gr.Sum(w => w.Wohnung.Nutzeinheit);
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
                    (double)PersonenIntervall.Where(p => p.Beginn <= w.Beginn).First().Personenzahl / w.Personenzahl *
                    (((double)(w.Ende - w.Beginn).Days + 1) / b.Abrechnungszeitspanne))).ToList();

        public Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch
        {
            get
            {
                var VerbrauchList = Rechnungen
                    .Where(g => g.Schluessel == UmlageSchluessel.NachVerbrauch)
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


        public Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch => Rechnungen
        .Where(g => g.Schluessel == UmlageSchluessel.NachVerbrauch)
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
        public List<Heizkostenberechnung> Heizkosten => Rechnungen
            .Where(r => (int)r.Typ % 2 == 1)
            .Select(r => new Heizkostenberechnung(r, b))
            .ToList();
        public double GesamtBetragKalt => Rechnungen.Where(r => (int)r.Typ % 2 == 0).Sum(r => r.Betrag);
        public double BetragKalt => Rechnungen.Where(r => (int)r.Typ % 2 == 0).Aggregate(0.0, (a, r) =>
            r.Schluessel switch
            {
                UmlageSchluessel.NachWohnflaeche => a + r.Betrag * WFZeitanteil,
                UmlageSchluessel.NachNutzeinheit => a + r.Betrag * NEZeitanteil,
                UmlageSchluessel.NachPersonenzahl => a + PersZeitanteil.Aggregate(0.0, (a2, z) => a2 += z.Anteil * r.Betrag),
                UmlageSchluessel.NachVerbrauch => a + r.Betrag * checkVerbrauch(r.Typ),
                _ => a + 0, // TODO or throw something...
            });
        public double GesamtBetragWarm => Heizkosten.Sum(h => h.PauschalBetrag);
        public double BetragWarm => Heizkosten.Sum(h => h.Kosten);

        public Rechnungsgruppe(Betriebskostenabrechnung _b, List<Betriebskostenrechnung> gruppe)
        {
            Rechnungen = gruppe;
            b = _b;

            // Remove 5% of Heizkosten to allgStrom (they are added there)
            var allgStrom = Rechnungen.Find(r => r.Typ == Betriebskostentyp.AllgemeinstromHausbeleuchtung);
            // TODO this 0.05 has to be variable.
            // TODO make sure this only happens only 1 time only.
            if (allgStrom != null && !b.AllgStromVerrechnetMitHeizkosten)
            {
                allgStrom.Betrag -= b.Vertrag.Wohnung.Betriebskostenrechnungsgruppen
                    .Select(g => g.Rechnung)
                    .Where(g => g.BetreffendesJahr == b.Jahr && (int)g.Typ % 2 == 1)
                    .Select(r => r.Betrag)
                    .Sum() * 0.05;
                b.AllgStromVerrechnetMitHeizkosten = true;
            }
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
            return merged.Select(m => new PersonenZeitIntervall(m, parent)).ToList();
        }
    }

    public class PersonenZeitIntervall
    {
        public DateTime Beginn { get; }
        public DateTime Ende { get; }
        public int Tage => (Ende - Beginn).Days + 1;
        public int GesamtTage => (new DateTime(Ende.Year, 12, 31) - new DateTime(Ende.Year, 1, 1)).Days + 1;
        public int Personenzahl { get; }
        public Rechnungsgruppe Parent { get; }

        public PersonenZeitIntervall((DateTime b, DateTime e, int p) i, Rechnungsgruppe parent)
        {
            Beginn = i.b;
            Ende = i.e;
            Personenzahl = i.p;
            Parent = parent;
        }
    }

    public sealed class VerbrauchAnteil
    {
        public string Kennnummer;
        public Zaehlertyp Typ;
        public double Delta;
        public double Anteil;

        public VerbrauchAnteil(string kennnummer, Zaehlertyp typ, double delta, double anteil)
        {
            Kennnummer = kennnummer;
            Typ = typ;
            Delta = delta;
            Anteil = anteil;
        }
    }
}
