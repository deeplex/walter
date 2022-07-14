using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungListViewModel : IFilterViewModel
    {
        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Liste = new();
        public ObservableProperty<WohnungListViewModelEntry> SelectedWohnung = new();

        public ObservableProperty<string> Filter { get; set; } = new();
        public ImmutableList<WohnungListViewModelEntry> AllRelevant { get; }

        public WohnungListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.Wohnungen
                .Include(w => w.Anhaenge)
                .Include(w => w.Adresse)
                .ThenInclude(a => a.Anhaenge)
                .Select(w => new WohnungListViewModelEntry(w, db))
                .ToImmutableList();

            Liste.Value = AllRelevant;
        }
    }
}
