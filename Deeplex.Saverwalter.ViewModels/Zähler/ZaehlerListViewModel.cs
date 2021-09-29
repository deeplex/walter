using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
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

        public ZaehlerListViewModel(AppViewModel avm)
        {
            AllRelevant = avm.ctx.ZaehlerSet
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
        // TODO remove i18n from viewmodels
        public string LastStandString => LastStand == null ? "Keine Angabe" : LastStand.Stand.ToString();
        public DateTime? Datum => LastStand == null ? null : LastStand.Datum;
        public int WohnungId => Entity.Wohnung?.WohnungId ?? 0;
        // TODO remove i18n from viewmodels
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