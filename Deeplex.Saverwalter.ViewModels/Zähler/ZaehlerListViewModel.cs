using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerListViewModel : ListViewModel<ZaehlerListViewModelEntry>, IListViewModel<ZaehlerListViewModelEntry>
    {
        public override string ToString() => "Zähler";

        protected override ImmutableList<ZaehlerListViewModelEntry> updateList(string filter)
            => AllRelevant.Where(v => applyFilter(filter, v.Kennnummer, v.TypString, v.Wohnung)).ToImmutableList();

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }

        public void SetList()
        {
            AllRelevant = transform(WalterDbService, include(WalterDbService));
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Wohnung w)
        {
            AllRelevant = transform(WalterDbService,
                include(WalterDbService)
                .Where(z => z.Wohnung != null && z.Wohnung.WohnungId == w.WohnungId)
                .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }


        private ZaehlerListViewModel(IWalterDbService db, INotificationService ns)
        {
            Navigate = new RelayCommand(el => ns.Navigation((Zaehler)el), _ => true);
        }

        private List<Zaehler> include(IWalterDbService db)
        {
            return db.ctx.ZaehlerSet
               .Include(z => z.Anhaenge)
               .Include(z => z.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
               .Include(z => z.Wohnung).ThenInclude(w => w.Anhaenge)
               .Include(z => z.Staende).ThenInclude(s => s.Anhaenge)
               .ToList();
        }
        private ImmutableList<ZaehlerListViewModelEntry> transform(IWalterDbService db, List<Zaehler> list)
        {
            return list
                .Select(z => new ZaehlerListViewModelEntry(z))
                .ToImmutableList();
        }
    }
}