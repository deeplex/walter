using Deeplex.Saverwalter.Model;
using System.Collections.Generic;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageListViewModelEntry
    {
        public Umlage Entity { get; }
        public int Id => Entity.UmlageId;
        public string Beschreibung => Entity.Beschreibung;
        public List<Wohnung> Wohnungen { get; }
        public Betriebskostentyp Typ => Entity.Typ;
        public string AdressenBezeichnung { get; }

        public int Tmpl { get; } = 0;

        public UmlageListViewModelEntry(Umlage r)
        {
            Entity = r;
            Wohnungen = r.Wohnungen.ToList();
            AdressenBezeichnung = Entity.Wohnungen.GetWohnungenBezeichnung();
        }

        public UmlageListViewModelEntry(Umlage r, int tmpl)
            : this(r)
        {
            Tmpl = tmpl;
        }
    }
}
