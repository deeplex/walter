using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<ZaehlerListViewModelEntry>> Liste = new ObservableProperty<ImmutableList<ZaehlerListViewModelEntry>>();
        private ZaehlerListViewModelEntry mSelectedZaehler;
        public ZaehlerListViewModelEntry SelectedZaehler
        {
            get => mSelectedZaehler;
            set
            {
                mSelectedZaehler = value;
                RaisePropertyChangedAuto();
            }
        }

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<ZaehlerListViewModelEntry> AllRelevant { get; }

        public ZaehlerListViewModel(AppViewModel avm)
        {
            AllRelevant = avm.ctx.ZaehlerSet
                .Include(z => z.Wohnung)
                .ThenInclude(w => w.Adresse)
                .Include(z => z.Staende)
                .Select(z => new ZaehlerListViewModelEntry(z))
                .ToImmutableList();
            Liste.Value = AllRelevant;
        }
    }
}