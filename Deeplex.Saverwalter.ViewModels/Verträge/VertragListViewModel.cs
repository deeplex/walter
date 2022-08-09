using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragListViewModel : ListViewModel<VertragListViewModelVertrag>, IListViewModel
    {
        public override string ToString() => "Verträge";

        protected override ImmutableList<VertragListViewModelVertrag> updateList(string filter = "")
            => AllRelevant.Where(v => applyFilter(filter, v.AnschriftMitWohnung, v.AuflistungMieter)).ToImmutableList();

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }

        public VertragListViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;
            Navigate = new RelayCommand(el => ns.Navigation((Vertrag)el), _ => true);
        }

        public void SetList()
        {
            AllRelevant = transform(WalterDbService, include(WalterDbService));
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(IPerson p)
        {
            var mieterSets = WalterDbService.ctx.MieterSet.ToList();

            AllRelevant = transform(WalterDbService, include(WalterDbService).Where(v =>
                v.AnsprechpartnerId == p.PersonId ||
                v.Wohnung.BesitzerId == p.PersonId ||
                mieterSets.Exists(w => w.VertragId == v.VertragId && w.PersonId == p.PersonId)
                ).ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Wohnung w)
        {
            AllRelevant = transform(WalterDbService, include(WalterDbService).Where(v => v.Wohnung.WohnungId == w.WohnungId).ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        private List<Vertrag> include(IWalterDbService db)
        {
            return db.ctx.Vertraege
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(v => v.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(v => v.Anhaenge)
                .ToList();
        }

        private ImmutableList<VertragListViewModelVertrag> transform(IWalterDbService db, List<Vertrag> list)
        {
            return list.GroupBy(v => v.VertragId)
                .Select(v => new VertragListViewModelVertrag(v, db))
                .OrderBy(v => v.Beginn).Reverse()
                .ToImmutableList();
        }
    }
}
