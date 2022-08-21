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
    public sealed class VertragListViewModel : ListViewModel<VertragListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Verträge";

        protected override void updateList()
        {
            List.Value = AllRelevant.Where(v => applyFilter(v.AnschriftMitWohnung, v.AuflistungMieter)).ToImmutableList();
        }

        public VertragListViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;
            Navigate = new RelayCommand(el => ns.Navigation((Vertrag)el), _ => true);
        }

        public override void SetList()
        {
            AllRelevant = transform(include(WalterDbService));
            updateList();
        }

        public void SetList(IPerson p)
        {
            if (p == null) return;

            var mieterSets = WalterDbService.ctx.MieterSet.ToList();

            AllRelevant = transform(include(WalterDbService).Where(v =>
                v.AnsprechpartnerId == p.PersonId ||
                v.Wohnung.BesitzerId == p.PersonId ||
                mieterSets.Exists(w => w.Vertrag.VertragId == v.VertragId && w.PersonId == p.PersonId)
                ).ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Wohnung w)
        {
            if (w == null) return;

            AllRelevant = transform(include(WalterDbService).Where(v => v.Wohnung.WohnungId == w.WohnungId).ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        private List<Vertrag> include(IWalterDbService db)
        {
            return db.ctx.Vertraege
                .Include(v => v.Mieten)
                .Include(v => v.Mietminderungen)
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(v => v.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(v => v.Anhaenge)
                .ToList();
        }

        private ImmutableList<VertragListViewModelEntry> transform(List<Vertrag> list)
        {
            return list
                .Select(v => new VertragListViewModelEntry(v, WalterDbService, NotificationService))
                .ToImmutableList();
        }
    }
}
