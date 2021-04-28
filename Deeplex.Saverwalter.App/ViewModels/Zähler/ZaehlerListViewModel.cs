using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels.Zähler
{
    public class ZaehlerListViewModel : BindableBase
    {
        public ObservableProperty<List<ZaehlerListEntry>> Liste = new ObservableProperty<List<ZaehlerListEntry>>();
        private ZaehlerListEntry mSelectedZaehler;
        public ZaehlerListEntry SelectedZaehler
        {
            get => mSelectedZaehler;
            set
            {
                mSelectedZaehler = value;
                RaisePropertyChangedAuto();
            }
        }

        public ZaehlerListViewModel()
        {
            Liste.Value = App.Walter.ZaehlerSet
                .Select(z => new ZaehlerListEntry(z))
                .ToList();
        }
    }

    public class ZaehlerListEntry
    {
        private Zaehler Entity;
        private Zaehlerstand LastStand;
        public int Id => Entity.ZaehlerId;
        public string Kennnummer => Entity.Kennnummer;
        public string TypString => Entity.Typ.ToString();
        public string LastStandString => LastStand == null ? "Keine Angabe" : LastStand.Stand.ToString();
        public string DatumString => LastStand == null ? null : LastStand.Datum.ToString("dd.MM.yyyy");
        public int WohnungId => Entity.Wohnung?.WohnungId ?? 0;
        public string Wohnung => Entity.Wohnung == null ? "Keine Wohnung" :
            AdresseViewModel.Anschrift(Entity.Wohnung) + ", " + Entity.Wohnung.Bezeichnung;
        public Zaehler AllgemeinZaehler => Entity.AllgemeinZaehler;

        public ZaehlerListEntry(Zaehler z)
        {
            Entity = z;
            LastStand = z.Staende.OrderBy(e => e.Datum).LastOrDefault();
        }
    }
}