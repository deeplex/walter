using Deeplex.Saverwalter.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungListViewModel : ListViewModel<WohnungListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Wohnungen";

        protected override ImmutableList<WohnungListViewModelEntry> updateList(string filter)
            => List.Value.Where(v => applyFilter(filter, v.Bezeichnung, v.Anschrift)).ToImmutableList();

        public WohnungListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.Wohnungen
                .Include(w => w.Anhaenge)
                .Include(w => w.Adresse)
                .ThenInclude(a => a.Anhaenge)
                .Select(w => new WohnungListViewModelEntry(w, db))
                .ToImmutableList();

            List.Value = AllRelevant;
        }
    }
}
