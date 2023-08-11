using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public class Abrechnungseinheit
    {
        public Dictionary<Umlage, Betriebskostenrechnung?> Rechnungen { get; } = new();
        public double BetragKalt { get; }
        public double BetragWarm { get; }
        public double GesamtBetragKalt { get; }
        public double GesamtBetragWarm { get; }
        public string Bezeichnung { get; }
        public double GesamtWohnflaeche { get; }
        public double GesamtNutzflaeche { get; }
        public int GesamtEinheiten { get; }
        public double WFZeitanteil { get; }
        public double NFZeitanteil { get; }
        public double NEZeitanteil { get; }
        public List<VerbrauchAnteil> VerbrauchAnteile { get; }
        public List<PersonenZeitanteil> PersonenZeitanteile { get; }
        public List<Heizkostenberechnung> Heizkostenberechnungen { get; }
        public double AllgStromFaktor { get; }

        public Abrechnungseinheit(List<Umlage> umlagen, Vertrag vertrag, Zeitraum zeitraum, List<Note> notes)
        {
            var wohnungen = umlagen.First().Wohnungen.ToList();
            Bezeichnung = umlagen.First().GetWohnungenBezeichnung();

            GesamtWohnflaeche = wohnungen.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = wohnungen.Sum(w => w.Nutzflaeche);
            GesamtEinheiten = wohnungen.Sum(w => w.Nutzeinheit);

            var wohnung = vertrag.Wohnung;
            WFZeitanteil = wohnung.Wohnflaeche / GesamtWohnflaeche * zeitraum.Zeitanteil;
            NFZeitanteil = wohnung.Nutzflaeche / GesamtNutzflaeche * zeitraum.Zeitanteil;
            NEZeitanteil = (double)wohnung.Nutzeinheit / GesamtEinheiten * zeitraum.Zeitanteil;

            foreach (var umlage in umlagen)
            {
                Rechnungen[umlage] = umlage.Betriebskostenrechnungen
                    .SingleOrDefault(rechnung => rechnung.BetreffendesJahr == zeitraum.Jahr);

                if (Rechnungen[umlage] == null)
                {
                    notes.Add(new Note($"Keine Rechnung für {umlage.Typ.ToDescriptionString()} gefunden.", Severity.Warning));
                }
            }

            var rechnungenKalt = Rechnungen
                .Where(e => (int)e.Key.Typ % 2 == 0 && e.Value != null)
                .Select(e => e.Value!)
                .ToList();
            var rechnungenWarm = Rechnungen
                .Where(e => (int)e.Key.Typ % 2 == 1 && e.Value != null)
                .Select(e => e.Value!)
                .ToList();

            PersonenZeitanteile = PersonenZeitanteil.GetPersonenZeitanteile(vertrag, wohnungen, zeitraum);
            VerbrauchAnteile = VerbrauchAnteil.GetVerbrauchAnteile(umlagen, vertrag, zeitraum, notes);

            Heizkostenberechnungen = CalculateHeizkosten(rechnungenWarm, wohnung, zeitraum, notes);
            BetragWarm = Heizkostenberechnungen.Sum(heizkosten => heizkosten.Betrag);
            GesamtBetragWarm = rechnungenWarm.Sum(e => e.Betrag);

            AllgStromFaktor = GesamtBetragWarm * 0.05;

            if (AllgStromFaktor > 0)
            {
                var allgStrom = rechnungenKalt
                    .SingleOrDefault(rechnung =>
                        rechnung.Umlage.Typ ==
                        Betriebskostentyp.AllgemeinstromHausbeleuchtung);

                if (allgStrom == null)
                {
                    notes.Add(new Note("Keine Allgemeinstromrechnung für Heizung gefunden", Severity.Error));
                }
                else if (allgStrom.Betrag < AllgStromFaktor)
                {
                    notes.Add(new Note($"Allgemeinstromrechnung ist niedriger als 5% der Heizungskosten. Rechnung wäre damit {allgStrom.Betrag - AllgStromFaktor}€", Severity.Warning));
                }
                else
                {
                    allgStrom.Betrag -= AllgStromFaktor;
                }
            }

            BetragKalt = GetSum(rechnungenKalt, notes);
            GesamtBetragKalt = rechnungenKalt.Sum(rechnung => rechnung.Betrag);

        }

        public double GetAnteil(Umlage umlage)
        {
            return umlage.Schluessel switch
            {
                Umlageschluessel.NachWohnflaeche => WFZeitanteil,
                Umlageschluessel.NachNutzflaeche => NFZeitanteil,
                Umlageschluessel.NachPersonenzahl => PersonenZeitanteile.Sum(anteil => anteil.Anteil),
                Umlageschluessel.NachVerbrauch => VerbrauchAnteile.Single(anteil => anteil.Umlage == umlage).Anteil.Sum(a => a.Value),
                _ => 0,
            };
        }

        private double GetSum(List<Betriebskostenrechnung> rechnungen, List<Note> notes)
        {
            var PersZeitanteil = PersonenZeitanteile.Sum(z => z.Anteil);

            return rechnungen.Sum(rechnung => rechnung.Umlage.Schluessel switch
            {
                Umlageschluessel.NachWohnflaeche => rechnung.Betrag * WFZeitanteil,
                Umlageschluessel.NachNutzeinheit => rechnung.Betrag * NEZeitanteil,
                Umlageschluessel.NachPersonenzahl => rechnung.Betrag * PersZeitanteil,
                Umlageschluessel.NachVerbrauch => rechnung.Betrag * GetVerbrauchAnteil(rechnung, notes),
                _ => 0
            });
        }

        private double GetVerbrauchAnteil(Betriebskostenrechnung rechnung, List<Note> notes)
        {
            var verbrauchAnteil = VerbrauchAnteile.Single(anteil => anteil.Umlage == rechnung.Umlage);
            if (verbrauchAnteil.Anteil.Count > 1)
            {
                notes.Add(new Note($"Verbrauch von Rechnung {rechnung.Umlage.Typ} enthält mehr als einen Zählertypen", Severity.Error));
                return 0;
            }
            return verbrauchAnteil.Anteil.First().Value;
        }

        public static List<Abrechnungseinheit> GetAbrechnungseinheiten(
            Vertrag vertrag,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            // Group up all Wohnungen sharing the same Umlage
            var einheiten = vertrag.Wohnung.Umlagen
                .GroupBy(umlage =>
                    new SortedSet<int>(umlage.Wohnungen.Select(gr => gr.WohnungId).ToList()),
                    new SortedSetIntEqualityComparer())
                .ToList();
            // Then create Rechnungsgruppen for every single one of those groups with respective information to calculate the distribution
            return einheiten
                .Select(einheit => new Abrechnungseinheit(einheit.ToList(), vertrag, zeitraum, notes))
                .ToList();
        }

        private static List<Heizkostenberechnung> CalculateHeizkosten(
            List<Betriebskostenrechnung> rechnungen,
            Wohnung wohnung,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return rechnungen
                .Select(rechnung => new Heizkostenberechnung(rechnung, wohnung, zeitraum, notes))
                .ToList();
        }

    }
}
