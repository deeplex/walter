using Deeplex.Saverwalter.Model;
using System.Collections.Generic;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageListViewModelEntry
    {
        public override string ToString()
        {
            var str = Wohnungen.GetWohnungenBezeichnung();
            return str.Trim() == string.Empty ? "Keine Wohnung" : str;
        }

        public Umlage Entity { get; }
        public int Id => Entity.UmlageId;
        public string Beschreibung => Entity.Beschreibung;
        public List<Wohnung> Wohnungen { get; }
        public Betriebskostentyp Typ => Entity.Typ;

        public int Tmpl { get; } = 0;

        public UmlageListViewModelEntry(Umlage r)
        {
            Entity = r;
            Wohnungen = r.Wohnungen.ToList();
        }

        public UmlageListViewModelEntry(Umlage r, int tmpl)
            : this(r)
        {
            Tmpl = tmpl;
        }
    }
}
