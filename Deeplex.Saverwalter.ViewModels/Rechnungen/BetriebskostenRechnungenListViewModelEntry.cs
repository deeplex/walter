using Deeplex.Saverwalter.Model;
using System.Collections.Generic;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenRechnungenListViewModelEntry
    {
        public override string ToString()
        {
            var str = Wohnungen.GetWohnungenBezeichnung();
            return str.Trim() == string.Empty ? "Keine Wohnung" : str;
        }

        public Betriebskostenrechnung Entity { get; }
        public int Id => Entity.BetriebskostenrechnungId;
        public List<Wohnung> Wohnungen { get; }
        public Betriebskostentyp Typ => Entity.Umlage.Typ;
        public int BetreffendesJahr => Entity.BetreffendesJahr;
        public double Betrag => Entity.Betrag;

        public int Tmpl { get; } = 0;

        public BetriebskostenRechnungenListViewModelEntry(Betriebskostenrechnung r)
        {
            Entity = r;
            Wohnungen = r.Umlage.Wohnungen.ToList();
        }

        public BetriebskostenRechnungenListViewModelEntry(Betriebskostenrechnung r, int tmpl)
            : this(r)
        {
            Tmpl = tmpl;
        }
    }
}
