using System.Collections.Immutable;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class VerbrauchAnteil
    {
        public Umlage Umlage { get; }
        public Dictionary<Zaehlereinheit, List<Verbrauch>> AlleZaehler { get; } = new();
        public Dictionary<Zaehlereinheit, double> AlleVerbrauch { get; } = new();
        public Dictionary<Zaehlereinheit, List<Verbrauch>> DieseZaehler { get; } = new();
        public Dictionary<Zaehlereinheit, double> DieseVerbrauch { get; } = new();
        public Dictionary<Zaehlereinheit, double> Anteil { get; } = new();

        public VerbrauchAnteil(Umlage umlage, Wohnung wohnung, Zeitraum zeitraum, List<Note> notes)
        {
            Umlage = umlage;
            var zaehlerGroups = umlage.Zaehler
                // if no Wohnung is attached => zaehler is allgemeinZaehler
                .Where(zaehler => zaehler.Wohnung != null)
                .GroupBy(zaehler => zaehler.Typ.ToUnit());

            foreach (var zaehlergroup in zaehlerGroups)
            {
                var unit = zaehlergroup.Key;

                AlleZaehler[unit] = new();
                AlleVerbrauch[unit] = new();
                DieseZaehler[unit] = new();
                DieseVerbrauch[unit] = new();

                foreach (var zaehler in zaehlergroup)
                {
                    var verbrauch = new Verbrauch(zaehler, zeitraum, notes);
                    AlleZaehler[unit].Add(verbrauch);
                    AlleVerbrauch[unit] += verbrauch.Delta;

                    if (zaehler.Wohnung == wohnung)
                    {
                        DieseZaehler[unit].Add(verbrauch);
                        DieseVerbrauch[unit] += verbrauch.Delta;
                    }
                }
            }

            foreach (var zaehlerTyp in DieseVerbrauch)
            {
                Anteil[zaehlerTyp.Key] = zaehlerTyp.Value / AlleVerbrauch[zaehlerTyp.Key];

                if (double.IsNaN(Anteil[zaehlerTyp.Key]))
                {
                    Anteil[zaehlerTyp.Key] = 0;
                }
            }
        }

        public static List<VerbrauchAnteil> GetVerbrauchAnteile(
            List<Umlage> umlagen,
            Vertrag vertrag,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return umlagen
                .Where(umlage => umlage.Zaehler.Count > 0)
                .Select(umlage => new VerbrauchAnteil(umlage, vertrag.Wohnung, zeitraum, notes))
                .ToList();
        }
    }
}
