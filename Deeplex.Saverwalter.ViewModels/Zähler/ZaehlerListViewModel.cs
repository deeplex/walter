using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<ZaehlerListViewModelEntry>> Liste = new();
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

        public ObservableProperty<string> Filter { get; set; } = new();
        public ImmutableList<ZaehlerListViewModelEntry> AllRelevant { get; }

        public ZaehlerListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.ZaehlerSet
                .Include(z => z.Anhaenge)
                .Include(z => z.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(z => z.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(z => z.Staende).ThenInclude(s => s.Anhaenge)
                .Select(z => new ZaehlerListViewModelEntry(z))
                .ToImmutableList();
            Liste.Value = AllRelevant;
        }
    }
}