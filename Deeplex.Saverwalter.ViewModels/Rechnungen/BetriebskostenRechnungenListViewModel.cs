using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenRechnungenListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<BetriebskostenRechnungenListEntry>> Liste =
            new ObservableProperty<ImmutableList<BetriebskostenRechnungenListEntry>>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ObservableProperty<int> JahrFilter { get; set; } = new ObservableProperty<int>();
        public ImmutableList<BetriebskostenRechnungenListEntry> AllRelevant { get; set; }

        private BetriebskostenRechnungenListEntry mSelectedRechnung;
        public BetriebskostenRechnungenListEntry SelectedRechnung
        {
            get => mSelectedRechnung;
            set
            {
                mSelectedRechnung = value;
                RaisePropertyChangedAuto();
            }
        }

        public BetriebskostenRechnungenListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .ThenInclude(a => a.Wohnungen)
                .Include(b => b.Zaehler)
                .Select(w => new BetriebskostenRechnungenListEntry(w))
                .ToImmutableList();
            Liste.Value = AllRelevant;
        }
    }
}
