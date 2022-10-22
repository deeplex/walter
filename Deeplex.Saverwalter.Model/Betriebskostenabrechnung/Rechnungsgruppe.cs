using System;
using System.Collections.Generic;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;

namespace Deeplex.Saverwalter.Model
{
    // Determine the fraction of people for a specific Rechnung with the fraction of time
    // List<(DateTime Beginn, DateTime Ende, double Anteil)> PersZeitanteil { get; }
    public class PersonenZeitanteil
    {
        public DateTime Beginn { get; }
        public DateTime Ende { get; }
        public double Anteil { get; }
        public int Personenzahl { get; }

        public PersonenZeitanteil(PersonenZeitIntervall interval, List<PersonenZeitIntervall> l, IBetriebskostenabrechnung b)
        {
            Beginn = interval.Beginn;
            Ende = interval.Ende;
            Personenzahl = interval.Personenzahl;

            double personenAnteil = (double)(l.FirstOrDefault(p => p.Beginn <= Beginn)?.Personenzahl ?? 0) / Personenzahl;
            double zeitAnteil = (double)((Ende - Beginn).Days + 1) / b.Abrechnungszeitspanne;

            Anteil = personenAnteil * zeitAnteil;
        }
    }

    public interface IRechnungsgruppe
    {
        string Bezeichnung { get; }
        List<PersonenZeitIntervall> PersonenIntervall { get; }
        List<PersonenZeitIntervall> GesamtPersonenIntervall { get; }
        double GesamtWohnflaeche { get; }
        double GesamtNutzflaeche { get; }
        int GesamtEinheiten { get; }
        double WFZeitanteil { get; } // Wohnflächenzeitanteil
        double NFZeitanteil { get; } // Nutzflächenzeitanteil
        double NEZeitanteil { get; } // Nutzeinheitenzeitanteil
        List<Umlage> Umlagen { get; }
        List<PersonenZeitanteil> PersonenZeitanteil { get; }
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
        private List<Wohnung> Wohnungen => Umlagen.First().Wohnungen.ToList();
        private IEnumerable<VertragVersion> alleVertraege => Wohnungen
            .SelectMany(w => w.Vertraege.SelectMany(e => e.Versionen))
            .Where(v => v.Beginn <= b.Abrechnungsende && (v.Ende() is null || v.Ende() >= b.Abrechnungsbeginn));

        public string Bezeichnung => Umlagen.First().GetWohnungenBezeichnung();
        public double GesamtWohnflaeche => Wohnungen.Sum(w => w.Wohnflaeche);
        public double WFZeitanteil => b.Wohnung.Wohnflaeche / GesamtWohnflaeche * b.Zeitanteil;
        public double NFZeitanteil => b.Wohnung.Nutzflaeche / GesamtNutzflaeche * b.Zeitanteil;
        public double GesamtNutzflaeche => Wohnungen.Sum(w => w.Nutzflaeche);
        public int GesamtEinheiten => Wohnungen.Sum(w => w.Nutzeinheit);
        public double NEZeitanteil => (double)b.Wohnung.Nutzeinheit / GesamtEinheiten * b.Zeitanteil;
        public List<PersonenZeitIntervall> GesamtPersonenIntervall
            => VertraegeIntervallPersonenzahl(alleVertraege, b, this).ToList();

        public List<PersonenZeitIntervall> PersonenIntervall
            => VertraegeIntervallPersonenzahl(b.Versionen, b, this).ToList();

        public List<PersonenZeitanteil> PersonenZeitanteil => GesamtPersonenIntervall
            .Where(g => g.Beginn < b.Nutzungsende && g.Ende >= b.Nutzungsbeginn)
            .Select((w, i) => new PersonenZeitanteil(w, PersonenIntervall, b)).ToList();

        public Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch
        {
            get
            {
                var VerbrauchList = Umlagen
                    .Where(g => g.Schluessel == Umlageschluessel.NachVerbrauch)
                    .Select(r => b.GetVerbrauch(r));

                if (VerbrauchList.Any(w => w.Count() == 0))
                {
                    // TODO this can be made even more explicit.
                    b.notes.Add(new Note("Für eine Rechnung konnte keine Zuordnung erstellt werden.", Severity.Error));
                    return new Dictionary<Betriebskostentyp, List<VerbrauchAnteil>>();
                }

                return VerbrauchList
                    .Where(r => r.Count > 0 && GesamtVerbrauch.ContainsKey(r.First().Betriebskostentyp))
                    .ToDictionary(r => r.First().Betriebskostentyp, r => r.Select(rr => new VerbrauchAnteil(
                        rr.Kennnummer,
                        rr.Zaehlertyp,
                        rr.Delta,
                        rr.Delta / GesamtVerbrauch[r.First().Betriebskostentyp].First(rrr => rrr.Typ == rr.Zaehlertyp).Delta))
                    .ToList());
            }
        }


        public Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch => Umlagen
            .Where(g => g.Schluessel == Umlageschluessel.NachVerbrauch)
            .Select(r => b.GetVerbrauch(r, true))
            .Where(g => g.Count > 0)
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
            .Sum(r => r.Umlage.Schluessel switch
            {
                Umlageschluessel.NachWohnflaeche => r.Betrag * WFZeitanteil,
                Umlageschluessel.NachNutzeinheit => r.Betrag * NEZeitanteil,
                Umlageschluessel.NachPersonenzahl => PersonenZeitanteil.Sum(z => z.Anteil * r.Betrag),
                Umlageschluessel.NachVerbrauch => r.Betrag * checkVerbrauch(r.Umlage.Typ),
                _ => 0
            });
        public double GesamtBetragWarm => Heizkosten.Sum(h => h.PauschalBetrag);
        public double BetragWarm => Heizkosten.Sum(h => h.Kosten);

        public Rechnungsgruppe(IBetriebskostenabrechnung _b, List<Umlage> gruppe)
        {
            Umlagen = gruppe;
            b = _b;
        }

        private static List<PersonenZeitIntervall> VertraegeIntervallPersonenzahl(
            IEnumerable<VertragVersion> vertraege, IBetriebskostenabrechnung b, Rechnungsgruppe parent)
        {
            var merged = vertraege
                .Where(v => v.Beginn <= b.Abrechnungsende && (v.Ende() is null || v.Ende() >= b.Abrechnungsbeginn))
                .SelectMany(v => new[]
                {
                    (Max(v.Beginn, b.Abrechnungsbeginn), v.Personenzahl),
                    (Min(v.Ende() ?? b.Abrechnungsende, b.Abrechnungsende).AddDays(1), -v.Personenzahl)
                })
                .GroupBy(t => t.Item1.Date)
                .Select(g => new PersonenZeitIntervall(g.Key, b.Abrechnungsende, g.Sum(t => t.Item2), parent))
                .OrderBy(t => t.Beginn)
                .ToList();

            for (int i = 0; i < merged.Count; ++i)
            {
                merged[i] = new PersonenZeitIntervall(
                    merged[i].Beginn,
                    i + 1 < merged.Count ? merged[i + 1].Beginn.AddDays(-1) : b.Abrechnungsende,
                    i - 1 >= 0 ? merged[i - 1].Personenzahl + merged[i].Personenzahl : merged[i].Personenzahl, parent);
            }
            if (merged.Count > 0)
            {
                merged.RemoveAt(merged.Count - 1);
            }

            // TODO refactor function to switch from tuple to class - or replace this function by constructor
            var ret = merged.Select(m => new PersonenZeitIntervall(m.Beginn, m.Ende, m.Personenzahl, parent)).ToList();

            return ret;
        }
    }
}
