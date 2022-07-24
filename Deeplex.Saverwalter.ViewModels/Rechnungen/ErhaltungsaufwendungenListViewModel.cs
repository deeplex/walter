using Deeplex.Saverwalter.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenListViewModel : ListViewModel<ErhaltungsaufwendungenListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Erhaltungsaufwendungen";

        protected override ImmutableList<ErhaltungsaufwendungenListViewModelEntry> updateList(string filter)
            => List.Value.Where(v => applyFilter(filter, v.Wohnung.Anschrift, v.Bezeichnung)).ToImmutableList();

        public ErhaltungsaufwendungenListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.Erhaltungsaufwendungen
                .Include(e => e.Anhaenge)
                .Include(e => e.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(e => e.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(w => w.Anhaenge)
                .Select(w => new ErhaltungsaufwendungenListViewModelEntry(w, db))
                .ToImmutableList();

            List.Value = AllRelevant;
        }
    }
}
