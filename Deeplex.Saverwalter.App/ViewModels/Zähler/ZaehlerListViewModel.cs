using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels.Zähler
{
    public class ZaehlerListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<ZaehlerListEntry>> Liste = new ObservableProperty<ImmutableList<ZaehlerListEntry>>();
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

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<ZaehlerListEntry> AllRelevant { get; }

        public ZaehlerListViewModel()
        {
            AllRelevant= App.Walter.ZaehlerSet
                .Include(z => z.Wohnung)
                .ThenInclude(w => w.Adresse)
                .Include(z => z.Staende)
                .Select(z => new ZaehlerListEntry(z))
                .ToImmutableList();
            Liste.Value = AllRelevant;
        }
    }

    public class ZaehlerListEntry
    {
        public override string ToString()
        {
            return Kennnummer + ", " + Wohnung;
        }

        public Zaehler Entity;
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