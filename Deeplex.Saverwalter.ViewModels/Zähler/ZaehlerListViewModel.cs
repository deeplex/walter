using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerListViewModel : ListViewModel<ZaehlerListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Zähler";

        protected override ImmutableList<ZaehlerListViewModelEntry> updateList(string filter)
            => List.Value.Where(v => applyFilter(filter, v.Kennnummer, v.TypString, v.Wohnung)).ToImmutableList();

        public ZaehlerListViewModel(IWalterDbService db, INotificationService ns): this(ns)
        {
            AllRelevant = transform(db, include(db));
            List.Value = AllRelevant;
        }

        public ZaehlerListViewModel(IWalterDbService db, INotificationService ns, Wohnung w) : this(ns)
        {
            AllRelevant = transform(db,
                include(db)
                .Where(z => z.Wohnung != null && z.Wohnung.WohnungId == w.WohnungId)
                .ToList());
            List.Value = AllRelevant;
        }


        private ZaehlerListViewModel(INotificationService ns)
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