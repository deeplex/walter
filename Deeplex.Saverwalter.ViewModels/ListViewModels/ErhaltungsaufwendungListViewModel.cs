using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungListViewModel : ListViewModel<ErhaltungsaufwendungListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Erhaltungsaufwendungen";

        protected override void updateList()
        {
            List.Value = AllRelevant.Where(v => applyFilter(v.Wohnung.Anschrift, v.Bezeichnung)).ToImmutableList();
        }

        public ErhaltungsaufwendungListViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;
            Navigate = new RelayCommand(el => ns.Navigation((Erhaltungsaufwendung)el), _ => true);
        }

        public override void SetList()
        {
            AllRelevant = transform(include(WalterDbService));
            updateList();
        }

        public void SetList(Wohnung w)
        {
            if (w == null) return;

            AllRelevant = transform(include(WalterDbService)
                    .Where(e => w.Erhaltungsaufwendungen
                    .Exists(i => i.ErhaltungsaufwendungId == e.ErhaltungsaufwendungId))
                    .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        private List<Erhaltungsaufwendung> include(IWalterDbService db)
        {
            return db.ctx.Erhaltungsaufwendungen
                .Include(e => e.Anhaenge)
                .Include(e => e.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(e => e.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(w => w.Anhaenge)
                .ToList();
        }
        private ImmutableList<ErhaltungsaufwendungListViewModelEntry> transform(List<Erhaltungsaufwendung> list)
        {
            return list.Select(w => new ErhaltungsaufwendungListViewModelEntry(w, WalterDbService)).ToImmutableList();
        }
    }
}
