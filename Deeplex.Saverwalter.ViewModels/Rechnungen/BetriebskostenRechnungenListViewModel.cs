using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenRechnungenListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<BetriebskostenRechnungenListEntry>> Liste = new();

        public ObservableProperty<string> Filter { get; set; } = new();
        public ObservableProperty<int> JahrFilter { get; set; } = new();
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
                .Include(b => b.Anhaenge)
                .Include(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Wohnungen).ThenInclude(w => w.Anhaenge)
                .Include(b => b.Zaehler).ThenInclude(b => b.Anhaenge)
                .Select(w => new BetriebskostenRechnungenListEntry(w))
                .ToImmutableList();
            Liste.Value = AllRelevant;
        }
    }
}
