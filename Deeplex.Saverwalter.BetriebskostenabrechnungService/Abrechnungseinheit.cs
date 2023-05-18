using Deeplex.Saverwalter.Model;
using System.Collections.Immutable;
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.Utils;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public interface IAbrechnungseinheit
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
        double BetragKalteNebenkosten { get; }
        double GesamtBetragKalteNebenkosten { get; }
        List<Heizkostenberechnung> Heizkosten { get; }
        double BetragWarmeNebenkosten { get; }
        double GesamtBetragWarmeNebenkosten { get; }
    }

    public sealed class Abrechnungseinheit : IAbrechnungseinheit
    {
        public List<Umlage> Umlagen { get; }
        private List<Wohnung> Wohnungen { get; }
        private List<VertragVersion> alleVertraege { get; }
        public string Bezeichnung { get; }

        public double GesamtBetragKalteNebenkosten { get; }

        public double GesamtWohnflaeche { get; }
        public double GesamtNutzflaeche { get; }
        public int GesamtEinheiten { get; }
        public List<PersonenZeitIntervall> GesamtPersonenIntervall { get; }
        public Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch { get; }

        public List<PersonenZeitIntervall> PersonenIntervall { get; }
        public Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch { get; }

        public double WFZeitanteil { get; }
        public double NFZeitanteil { get; }
        public double NEZeitanteil { get; }
        public List<PersonenZeitanteil> PersonenZeitanteil { get; }
        public Dictionary<Betriebskostentyp, double> VerbrauchAnteil { get; }

        public double BetragKalteNebenkosten { get; }

        public List<Heizkostenberechnung> Heizkosten { get; }
        public double GesamtBetragWarmeNebenkosten { get; }
        public double BetragWarmeNebenkosten { get; }

        public Abrechnungseinheit(
            List<Umlage> umlagen,
            Wohnung wohnung,
            List<VertragVersion> versionen,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            Umlagen = umlagen;
            Wohnungen = umlagen.First().Wohnungen.ToList();
            alleVertraege = getAllVertragVersionen(Wohnungen, zeitraum);
            Bezeichnung = umlagen.First().GetWohnungenBezeichnung();

            var kalteNebenkosten = GetKalteNebenkosten(umlagen, zeitraum);
            GesamtBetragKalteNebenkosten = kalteNebenkosten.Sum(rechnung => rechnung.Betrag);

            GesamtWohnflaeche = Wohnungen.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = Wohnungen.Sum(w => w.Nutzflaeche);
            GesamtEinheiten = Wohnungen.Sum(w => w.Nutzeinheit);
            GesamtPersonenIntervall = VertraegeIntervallPersonenzahl(alleVertraege, zeitraum);
            GesamtVerbrauch = CalculateAbrechnungseinheitVerbrauch(umlagen, zeitraum, notes);

            PersonenIntervall = VertraegeIntervallPersonenzahl(versionen, zeitraum);
            Verbrauch = CalculateWohnungVerbrauch(umlagen, wohnung, zeitraum, GesamtVerbrauch, notes);

            WFZeitanteil = wohnung.Wohnflaeche / GesamtWohnflaeche * zeitraum.Zeitanteil;
            NFZeitanteil = wohnung.Nutzflaeche / GesamtNutzflaeche * zeitraum.Zeitanteil;
            NEZeitanteil = (double)wohnung.Nutzeinheit / GesamtEinheiten * zeitraum.Zeitanteil;
            PersonenZeitanteil = GetPersonenZeitanteil(PersonenIntervall, GesamtPersonenIntervall, zeitraum);
            VerbrauchAnteil = CalculateVerbrauchAnteil(Verbrauch);

            BetragKalteNebenkosten = CalculateBetragKalteNebenkosten(kalteNebenkosten, WFZeitanteil, NEZeitanteil, PersonenZeitanteil, VerbrauchAnteil, notes);

            Heizkosten = CalculateHeizkosten(umlagen, wohnung, zeitraum, notes);
            GesamtBetragWarmeNebenkosten = Heizkosten.Sum(heizkostenberechnung => heizkostenberechnung.PauschalBetrag);
            BetragWarmeNebenkosten = Heizkosten.Sum(heizkostenberechnung => heizkostenberechnung.Kosten);
        }

        private static List<VertragVersion> getAllVertragVersionen(List<Wohnung> wohnungen, Zeitraum zeitraum)
        {
            return wohnungen
                .SelectMany(w => w.Vertraege.SelectMany(e => e.Versionen))
                .ToList()
                .Where(v => v.Beginn <= zeitraum.Abrechnungsende && (v.Ende() is null || v.Ende() >= zeitraum.Abrechnungsbeginn))
                .ToList();
        }

        private static double checkVerbrauch(Dictionary<Betriebskostentyp, double> verbrauchAnteil, Betriebskostentyp typ, List<Note> notes)
        {
            if (verbrauchAnteil.ContainsKey(typ))
            {
                return verbrauchAnteil[typ];
            }
            else
            {
                notes.Add(new Note("Konnte keinen Anteil für " + typ.ToDescriptionString() + " feststellen.", Severity.Error));
                return 0;
            }
        }

        public static List<Verbrauch> GetVerbrauchAbrechnungseinheit(Umlage umlage, Zeitraum zeitraum, List<Note> notes)
        {
            var Zaehler = umlage.Zaehler.Where(z => z.Wohnung != null).ToList();
            // TODO check that all Zaehler have the Same typ
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(Zaehler, zeitraum.Nutzungsende);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(Zaehler, zeitraum.Nutzungsbeginn);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(umlage, ZaehlerAnfangsStaende, ZaehlerEndStaende, notes);

            return Deltas;
        }

        private static List<Verbrauch> GetVerbrauchWohnung(Umlage umlage, Wohnung wohnung, Zeitraum zeitraum, List<Note> notes)
        {
            var Zaehler = umlage.Zaehler.Where(z => z.Wohnung == wohnung).ToList();
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(Zaehler, zeitraum.Nutzungsende);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(Zaehler, zeitraum.Nutzungsbeginn);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(umlage, ZaehlerAnfangsStaende, ZaehlerEndStaende, notes);

            return Deltas;
        }

        private static ImmutableList<Zaehlerstand> GetZaehlerEndStaendeFuerBerechnung(List<Zaehler> zaehler, DateOnly nutzungsende)
        {
            return zaehler
                .Select(z => z.Staende
                    .OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= nutzungsende && (nutzungsende.DayNumber - l.Datum.DayNumber) < 30))
                .ToList()
                .Where(zaehlerstand => zaehlerstand is not null)
                .Select(e => e!) // still necessary to make sure not null?
                .ToImmutableList() ?? new List<Zaehlerstand>() { }.ToImmutableList();
        }

        private static ImmutableList<Zaehlerstand> GetZaehlerAnfangsStaendeFuerBerechnung(List<Zaehler> zaehler, DateOnly nutzungsbeginn)
        {
            return zaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum)
                    .ToList()
                    .Where(l => Math.Abs(l.Datum.DayNumber - nutzungsbeginn.AddDays(-1).DayNumber) < 30)
                    .ToList()
                .FirstOrDefault())
                .Where(zs => zs != null)
                .Select(e => e!) // still necessary to make sure not null?
                .ToImmutableList();
        }

        private static List<Verbrauch> GetVerbrauchForZaehlerStaende(Umlage umlage, ImmutableList<Zaehlerstand> beginne, ImmutableList<Zaehlerstand> enden, List<Note> notes)
        {
            List<Verbrauch> Deltas = new();

            if (enden.IsEmpty)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (beginne.IsEmpty)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (enden.Count != beginne.Count)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else
            {
                for (var i = 0; i < enden.Count; ++i)
                {
                    Deltas.Add(new Verbrauch(umlage.Typ, enden[i].Zaehler.Kennnummer, enden[i].Zaehler.Typ, enden[i].Stand - beginne[i].Stand));
                }
            }

            return Deltas;
        }

        private static Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> CalculateWohnungVerbrauch(
            List<Umlage> Umlagen,
            Wohnung wohnung,
            Zeitraum zeitraum,
            Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch,
            List<Note> notes)
        {
            var VerbrauchList = Umlagen
                    .Where(g => g.Schluessel == Umlageschluessel.NachVerbrauch)
                    .ToList()
                    .Select(umlage => GetVerbrauchWohnung(umlage, wohnung, zeitraum, notes))
                    .ToList();

            if (VerbrauchList.Any(w => w.Count == 0))
            {
                // TODO this can be made even more explicit.
                notes.Add(new Note("Für eine Rechnung konnte keine Zuordnung erstellt werden.", Severity.Error));
                VerbrauchList = VerbrauchList.Where(w => w.Count > 0).ToList();
            }

            return VerbrauchList
                .Where(verbrauchList => verbrauchList.Count > 0 && GesamtVerbrauch.ContainsKey(verbrauchList.First().Betriebskostentyp))
                .ToList()
                .ToDictionary(
                    verbrauchList => verbrauchList.First().Betriebskostentyp,
                    verbrauchList => verbrauchList.Select(verbrauch => new VerbrauchAnteil(
                        verbrauch.Kennnummer,
                        verbrauch.Zaehlertyp,
                        verbrauch.Delta,
                        verbrauch.Delta / GesamtVerbrauch[verbrauchList.First().Betriebskostentyp]
                            .First(typDeltaGroup => typDeltaGroup.Typ == verbrauch.Zaehlertyp).Delta))
                .ToList());
        }

        private static List<PersonenZeitIntervall> VertraegeIntervallPersonenzahl(
            List<VertragVersion> vertraege,
            Zeitraum zeitraum)
        {
            var merged = vertraege
                .Where(vertragVersion =>
                    vertragVersion.Beginn <= zeitraum.Abrechnungsende
                    && (vertragVersion.Ende() is null || vertragVersion.Ende() >= zeitraum.Abrechnungsbeginn))
                .ToList()
                .SelectMany(v => new[]
                {
                    (Max(v.Beginn, zeitraum.Abrechnungsbeginn), v.Personenzahl),
                    (Min(v.Ende() ?? zeitraum.Abrechnungsende, zeitraum.Abrechnungsende).AddDays(1), -v.Personenzahl)
                })
                .ToList()
                .GroupBy(t => t.Item1)
                .ToList()
                .Select(g => new PersonenZeitIntervall(g.Key, zeitraum.Abrechnungsende, g.Sum(t => t.Item2)))
                .ToList()
                .OrderBy(t => t.Beginn)
                .ToList();

            for (int i = 0; i < merged.Count; ++i)
            {
                merged[i] = new PersonenZeitIntervall(
                    merged[i].Beginn,
                    i + 1 < merged.Count ? merged[i + 1].Beginn.AddDays(-1) : zeitraum.Abrechnungsende,
                    i - 1 >= 0 ? merged[i - 1].Personenzahl + merged[i].Personenzahl : merged[i].Personenzahl);
            }
            if (merged.Count > 0)
            {
                merged.RemoveAt(merged.Count - 1);
            }

            // TODO refactor function to switch from tuple to class - or replace this function by constructor
            var ret = merged.Select(personenZeitIntervall => new PersonenZeitIntervall(
                personenZeitIntervall.Beginn,
                personenZeitIntervall.Ende,
                personenZeitIntervall.Personenzahl))
                .ToList();

            return ret;
        }

        private static List<PersonenZeitanteil> GetPersonenZeitanteil(
            List<PersonenZeitIntervall> intervalle,
            List<PersonenZeitIntervall> gesamtIntervalle,
            Zeitraum zeitraum)
        {
            return intervalle
                .Where(g => g.Beginn < zeitraum.Nutzungsende && g.Ende >= zeitraum.Nutzungsbeginn)
                .ToList()
                .Select((w, i) => new PersonenZeitanteil(w, gesamtIntervalle, zeitraum))
                .ToList();
        }

        private static Dictionary<Betriebskostentyp, List<(Zaehlertyp, double)>> CalculateAbrechnungseinheitVerbrauch(
            List<Umlage> umlagen,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return umlagen
                .Where(umlage => umlage.Schluessel == Umlageschluessel.NachVerbrauch)
                .ToList()
                .Select(umlage => GetVerbrauchAbrechnungseinheit(umlage, zeitraum, notes))
                .ToList()
                .Where(verbrauchList => verbrauchList.Count > 0)
                .ToList()
                .ToDictionary(
                    verbrauchList => verbrauchList.First().Betriebskostentyp,
                    verbrauchList => verbrauchList.GroupBy(gg => gg.Zaehlertyp)
                .Select(typVerbrauchGrouping => (typVerbrauchGrouping.Key, typVerbrauchGrouping
                    .Sum(verbrauch => verbrauch.Delta)))
                .ToList());
        }

        private static Dictionary<Betriebskostentyp, double> CalculateVerbrauchAnteil(Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> verbrauch)
        {
            return verbrauch
                .Select(typVerbrauchAnteilPair => (
                    typVerbrauchAnteilPair.Key,
                    typVerbrauchAnteilPair.Value.Sum(verbrauchAnteil => verbrauchAnteil.Delta),
                    typVerbrauchAnteilPair.Value.Sum(verbrauchAnteil => verbrauchAnteil.Delta / verbrauchAnteil.Anteil)))
                .ToList()
                .ToDictionary(typ => typ.Key, typ => typ.Item2 / typ.Item3);
        }

        private static List<Heizkostenberechnung> CalculateHeizkosten(
            List<Umlage> umlagen,
            Wohnung wohnung,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return umlagen
                .Where(r => (int)r.Typ % 2 == 1)
                .ToList()
                .SelectMany(umlage => umlage.Betriebskostenrechnungen)
                .ToList()
                .Where(rechnung => rechnung.BetreffendesJahr == zeitraum.Jahr)
                .ToList()
                .Select(rechnung => new Heizkostenberechnung(rechnung, wohnung, zeitraum, notes))
                .ToList();
        }

        private static List<Betriebskostenrechnung> GetKalteNebenkosten(List<Umlage> umlagen, Zeitraum zeitraum)
        {
            return umlagen
                .Where(umlage => (int)umlage.Typ % 2 == 0)
                .ToList()
                .SelectMany(umlage => umlage.Betriebskostenrechnungen)
                .ToList()
                .Where(rechnung => rechnung.BetreffendesJahr == zeitraum.Jahr)
                .ToList();
        }

        private static double CalculateBetragKalteNebenkosten(
            List<Betriebskostenrechnung> kalteUmlagen,
            double wfZeitanteil,
            double neZeitanteil,
            List<PersonenZeitanteil>
            personenZeitanteil,
            Dictionary<Betriebskostentyp, double> verbrauchAnteil,
            List<Note> notes)
        {
            return kalteUmlagen.Sum(rechnung => rechnung.Umlage.Schluessel switch
            {
                Umlageschluessel.NachWohnflaeche => rechnung.Betrag * wfZeitanteil,
                Umlageschluessel.NachNutzeinheit => rechnung.Betrag * neZeitanteil,
                Umlageschluessel.NachPersonenzahl => personenZeitanteil.Sum(z => z.Anteil * rechnung.Betrag),
                Umlageschluessel.NachVerbrauch => rechnung.Betrag * checkVerbrauch(verbrauchAnteil, rechnung.Umlage.Typ, notes),
                _ => 0
            });
        }
    }
}
