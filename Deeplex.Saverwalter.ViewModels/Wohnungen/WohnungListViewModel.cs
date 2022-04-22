using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungListViewModel : IFilterViewModel
    {
        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Liste = new ObservableProperty<ImmutableList<WohnungListViewModelEntry>>();
        public ObservableProperty<WohnungListViewModelEntry> SelectedWohnung
            = new ObservableProperty<WohnungListViewModelEntry>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<WohnungListViewModelEntry> AllRelevant { get; }

        public WohnungListViewModel(AppViewModel avm)
        {
            AllRelevant = avm.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListViewModelEntry(w, avm))
                .ToImmutableList();

            Liste.Value = AllRelevant;
        }
    }
}
