using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragListViewModel : ListViewModel<VertragListViewModelVertrag>, IListViewModel
    {
        public override string ToString() => "Verträge";

        protected override ImmutableList<VertragListViewModelVertrag> updateList(string filter)
            => List.Value.Where(v => applyFilter(filter, v.AnschriftMitWohnung, v.AuflistungMieter)).ToImmutableList();

        public VertragListViewModel(IWalterDbService db, INotificationService ns)
        {
            Add = new RelayCommand(_ => ns.Navigation<Vertrag>(null), _ => true);

            AllRelevant = db.ctx.Vertraege
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(v => v.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(v => v.Anhaenge)
                .ToList()
                .GroupBy(v => v.VertragId)
                .Select(v => new VertragListViewModelVertrag(v, db))
                .OrderBy(v => v.Beginn).Reverse()
                .ToImmutableList();
            List.Value = AllRelevant;
        }
    }
}
