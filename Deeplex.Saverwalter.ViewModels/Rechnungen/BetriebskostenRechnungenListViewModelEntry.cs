using Deeplex.Saverwalter.Model;
using System.Collections.Generic;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenRechnungenListEntry
    {
        public Betriebskostenrechnung Entity { get; }
        public int Id => Entity.BetriebskostenrechnungId;
        public List<Wohnung> Wohnungen { get; }
        public Betriebskostentyp Typ => Entity.Umlage.Typ;
        public int BetreffendesJahr => Entity.BetreffendesJahr;
        public double Betrag => Entity.Betrag;
        public string AdressenBezeichnung { get; }

        public int Tmpl { get; } = 0;

        public BetriebskostenRechnungenListEntry(Betriebskostenrechnung r)
        {
            Entity = r;
            Wohnungen = r.Wohnungen.ToList();
            AdressenBezeichnung = Entity.GetWohnungenBezeichnung();
        }

        public BetriebskostenRechnungenListEntry(Betriebskostenrechnung r, int tmpl)
            : this(r)
        {
            Tmpl = tmpl;
        }
    }
}
