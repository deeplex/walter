using Deeplex.Saverwalter.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerListViewModel : ListViewModel<ZaehlerListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Zähler";

        protected override ImmutableList<ZaehlerListViewModelEntry> updateList(string filter)
            => List.Value.Where(v => applyFilter(filter, v.Kennnummer, v.TypString, v.Wohnung)).ToImmutableList();

        public ZaehlerListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.ZaehlerSet
                .Include(z => z.Anhaenge)
                .Include(z => z.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(z => z.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(z => z.Staende).ThenInclude(s => s.Anhaenge)
                .Select(z => new ZaehlerListViewModelEntry(z))
                .ToImmutableList();

            List.Value = AllRelevant;
        }
    }
}