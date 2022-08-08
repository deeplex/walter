using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageListViewModel : ListViewModel<UmlageListViewModelEntry>, IListViewModel<UmlageListViewModelEntry>
    {
        public override string ToString() => "Umlagen";

        protected override ImmutableList<UmlageListViewModelEntry> updateList(string filter)
            => AllRelevant.Where(v => applyFilter(filter, v.Typ.ToDescriptionString(), v.ToString())).ToImmutableList();

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }

        public UmlageListViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;
            Navigate = new RelayCommand(el => ns.Navigation((Umlage)el), _ => true);
        }

        public void SetList()
        {
            AllRelevant = transform(WalterDbService, include(WalterDbService));
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Vertrag v)
        {
            AllRelevant = transform(WalterDbService,
                include(WalterDbService)
                    .Where(b => b.Wohnungen.Exists(w => v.Wohnung.WohnungId == w.WohnungId))
                    .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Wohnung w)
        {
            AllRelevant = transform(WalterDbService,
                include(WalterDbService)
                    .Where(b => b.Wohnungen.Exists(i => i.WohnungId == w.WohnungId))
                    .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        private List<Umlage> include(IWalterDbService db)
        {
            return db.ctx.Umlagen
                .Include(b => b.Anhaenge)
                .Include(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Wohnungen).ThenInclude(w => w.Anhaenge)
                .ToList();
        }
        private ImmutableList<UmlageListViewModelEntry> transform(IWalterDbService db, List<Umlage> list)
        {
            return list
                .Select(w => new UmlageListViewModelEntry(w))
                .ToImmutableList();
        }
    }
}
