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

        public PersonenZeitanteil(
            PersonenZeitIntervall interval,
            List<PersonenZeitIntervall> personenZeitIntervallList,
            BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung)
        {
            Beginn = interval.Beginn;
            Ende = interval.Ende;
            Personenzahl = interval.Personenzahl;

            double personenAnteil = (double)(personenZeitIntervallList.FirstOrDefault(personenZeitIntervall =>
                personenZeitIntervall.Beginn <= Beginn)?.Personenzahl ?? 0) / Personenzahl;
            double zeitAnteil = (double)((Ende - Beginn).Days + 1) / betriebskostenabrechnung.Abrechnungszeitspanne;

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
        double GesamtBetragKalt { get; }
        List<Heizkostenberechnung> Heizkosten { get; }
        double BetragWarm { get; }
        double GesamtBetragWarm { get; }
    }

    public sealed class Rechnungsgruppe : IRechnungsgruppe
    {
        public List<Umlage> Umlagen { get; }

        private List<Wohnung> Wohnungen { get; }
        private List<VertragVersion> alleVertraege { get; }

        public string Bezeichnung { get; }
        public double GesamtWohnflaeche { get; }
        public double WFZeitanteil { get; }
        public double NFZeitanteil { get; }
        public double GesamtNutzflaeche { get; }
        public int GesamtEinheiten { get; }
        public double NEZeitanteil { get; }
        public List<PersonenZeitIntervall> GesamtPersonenIntervall { get; }
        public List<PersonenZeitIntervall> PersonenIntervall { get; }
        public List<PersonenZeitanteil> PersonenZeitanteil { get; }
        public Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch { get; }

        public Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch { get; }

        public Dictionary<Betriebskostentyp, double> VerbrauchAnteil { get; }
        public List<Heizkostenberechnung> Heizkosten { get; }
        public double GesamtBetragKalt { get; }
        public double BetragKalt { get; }
        public double GesamtBetragWarm { get; }
        public double BetragWarm { get; }

        public Rechnungsgruppe(SaverwalterContext ctx, BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebkostenabrechnung, List<Umlage> gruppe)
        {
            Umlagen = gruppe;
            Wohnungen = Umlagen.First().Wohnungen.ToList();
            alleVertraege = Wohnungen
                .SelectMany(w => w.Vertraege.SelectMany(e => e.Versionen))
                .ToList()
                .Where(v => v.Beginn <= betriebkostenabrechnung.Abrechnungsende && (v.Ende() is null || v.Ende() >= betriebkostenabrechnung.Abrechnungsbeginn))
                .ToList();
            Bezeichnung = Umlagen.First().GetWohnungenBezeichnung();
            GesamtWohnflaeche = Wohnungen.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = Wohnungen.Sum(w => w.Nutzflaeche);
            WFZeitanteil = betriebkostenabrechnung.Wohnung.Wohnflaeche / GesamtWohnflaeche * betriebkostenabrechnung.Zeitanteil;
            NFZeitanteil = betriebkostenabrechnung.Wohnung.Nutzflaeche / GesamtNutzflaeche * betriebkostenabrechnung.Zeitanteil;
            GesamtNutzflaeche = Wohnungen.Sum(w => w.Nutzflaeche);
            GesamtEinheiten = Wohnungen.Sum(w => w.Nutzeinheit);
            NEZeitanteil = (double)betriebkostenabrechnung.Wohnung.Nutzeinheit / GesamtEinheiten * betriebkostenabrechnung.Zeitanteil;
            GesamtPersonenIntervall = VertraegeIntervallPersonenzahl(alleVertraege, betriebkostenabrechnung, this).ToList();
            PersonenIntervall = VertraegeIntervallPersonenzahl(betriebkostenabrechnung.Versionen, betriebkostenabrechnung, this).ToList();

            PersonenZeitanteil = GesamtPersonenIntervall
                .Where(g => g.Beginn < betriebkostenabrechnung.Nutzungsende && g.Ende >= betriebkostenabrechnung.Nutzungsbeginn)
                .ToList()
                .Select((w, i) => new PersonenZeitanteil(w, PersonenIntervall, betriebkostenabrechnung))
                .ToList();

            GesamtVerbrauch = Umlagen
                .Where(umlage => umlage.Schluessel == Umlageschluessel.NachVerbrauch)
                .ToList()
                .Select(r => betriebkostenabrechnung.GetVerbrauchWohnung(ctx, r))
                .ToList()
                .Where(verbrauchList => verbrauchList.Count > 0)
                .ToList()
                .ToDictionary(
                    verbrauchList => verbrauchList.First().Betriebskostentyp,
                    verbrauchList => verbrauchList.GroupBy(gg => gg.Zaehlertyp)
                .Select(typVerbrauchGrouping => (typVerbrauchGrouping.Key, typVerbrauchGrouping
                    .Sum(verbrauch => verbrauch.Delta)))
                .ToList());
            Verbrauch = GetVerbrauch(ctx, betriebkostenabrechnung, Umlagen, GesamtVerbrauch);

            VerbrauchAnteil = Verbrauch
                .Select(typVerbrauchAnteilPair => (
                    typVerbrauchAnteilPair.Key,
                    typVerbrauchAnteilPair.Value.Sum(verbrauchAnteil => verbrauchAnteil.Delta),
                    typVerbrauchAnteilPair.Value.Sum(verbrauchAnteil => verbrauchAnteil.Delta / verbrauchAnteil.Anteil)))
                .ToList()
                .ToDictionary(typ => typ.Key, typ => typ.Item2 / typ.Item3);

            Heizkosten = Umlagen
                .Where(r => (int)r.Typ % 2 == 1)
                .ToList()
                .SelectMany(umlage => umlage.Betriebskostenrechnungen)
                .ToList()
                .Where(rechnung => rechnung.BetreffendesJahr == betriebkostenabrechnung.Jahr)
                .ToList()
                .Select(rechnung => new Heizkostenberechnung(ctx, rechnung, betriebkostenabrechnung))
                .ToList();

            var kalteUmlagen = Umlagen
                .Where(umlage => (int)umlage.Typ % 2 == 0)
                .ToList()
                .SelectMany(umlage => umlage.Betriebskostenrechnungen)
                .ToList()
                .Where(rechnung => rechnung.BetreffendesJahr == betriebkostenabrechnung.Jahr)
                .ToList();

            GesamtBetragKalt = kalteUmlagen.Sum(rechnung => rechnung.Betrag);
            BetragKalt = kalteUmlagen.Sum(rechnung => rechnung.Umlage.Schluessel switch
                {
                    Umlageschluessel.NachWohnflaeche => rechnung.Betrag * WFZeitanteil,
                    Umlageschluessel.NachNutzeinheit => rechnung.Betrag * NEZeitanteil,
                    Umlageschluessel.NachPersonenzahl => PersonenZeitanteil.Sum(z => z.Anteil * rechnung.Betrag),
                    Umlageschluessel.NachVerbrauch => rechnung.Betrag * checkVerbrauch(betriebkostenabrechnung, rechnung.Umlage.Typ),
                    _ => 0
                });

            GesamtBetragWarm = Heizkosten.Sum(heizkostenberechnung => heizkostenberechnung.PauschalBetrag);
            BetragWarm = Heizkosten.Sum(heizkostenberechnung => heizkostenberechnung.Kosten);
        }

        private double checkVerbrauch(BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung, Betriebskostentyp typ)
        {
            if (VerbrauchAnteil.ContainsKey(typ))
            {
                return VerbrauchAnteil[typ];
            }
            else
            {
                betriebskostenabrechnung.Notes.Add(new Note("Konnte keinen Anteil für " + typ.ToDescriptionString() + " feststellen.", Severity.Error));
                return 0;
            }
        }

        private static Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> GetVerbrauch(
            SaverwalterContext ctx,
            BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung,
            List<Umlage> Umlagen,
            Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch)
        {
            var VerbrauchList = Umlagen
                    .Where(g => g.Schluessel == Umlageschluessel.NachVerbrauch)
                    .ToList()
                    .Select(umlage => betriebskostenabrechnung.GetVerbrauchGanzeGruppe(ctx, umlage))
                    .ToList();

            if (VerbrauchList.Any(w => w.Count() == 0))
            {
                // TODO this can be made even more explicit.
                betriebskostenabrechnung.Notes.Add(new Note("Für eine Rechnung konnte keine Zuordnung erstellt werden.", Severity.Error));
                return new Dictionary<Betriebskostentyp, List<VerbrauchAnteil>>();
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
            BetriebskostenabrechnungService.IBetriebskostenabrechnung betriebskostenabrechnung,
            Rechnungsgruppe parent)
        {
            var merged = vertraege
                .Where(vertragVersion =>
                    vertragVersion.Beginn <= betriebskostenabrechnung.Abrechnungsende
                    && (vertragVersion.Ende() is null || vertragVersion.Ende() >= betriebskostenabrechnung.Abrechnungsbeginn))
                .ToList()
                .SelectMany(v => new[]
                {
                    (Max(v.Beginn, betriebskostenabrechnung.Abrechnungsbeginn), v.Personenzahl),
                    (Min(v.Ende() ?? betriebskostenabrechnung.Abrechnungsende, betriebskostenabrechnung.Abrechnungsende).AddDays(1), -v.Personenzahl)
                })
                .ToList()
                .GroupBy(t => t.Item1.Date)
                .ToList()
                .Select(g => new PersonenZeitIntervall(g.Key, betriebskostenabrechnung.Abrechnungsende, g.Sum(t => t.Item2), parent))
                .ToList()
                .OrderBy(t => t.Beginn)
                .ToList();

            for (int i = 0; i < merged.Count; ++i)
            {
                merged[i] = new PersonenZeitIntervall(
                    merged[i].Beginn,
                    i + 1 < merged.Count ? merged[i + 1].Beginn.AddDays(-1) : betriebskostenabrechnung.Abrechnungsende,
                    i - 1 >= 0 ? merged[i - 1].Personenzahl + merged[i].Personenzahl : merged[i].Personenzahl, parent);
            }
            if (merged.Count > 0)
            {
                merged.RemoveAt(merged.Count - 1);
            }

            // TODO refactor function to switch from tuple to class - or replace this function by constructor
            var ret = merged.Select(personenZeitIntervall => new PersonenZeitIntervall(
                personenZeitIntervall.Beginn,
                personenZeitIntervall.Ende,
                personenZeitIntervall.Personenzahl,
                parent))
                .ToList();

            return ret;
        }
    }
}
