using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageListViewModel : ListViewModel<UmlageListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Umlagen";

        protected override void updateList()
        {
            List.Value = AllRelevant.Where(v => applyFilter(v.Typ.ToDescriptionString(), v.ToString())).ToImmutableList();
        }

        public UmlageListViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;
            Navigate = new RelayCommand(el => ns.Navigation((Umlage)el), _ => true);
        }

        public override void SetList()
        {
            AllRelevant = transform(include(WalterDbService));
            updateList();
        }

        public void SetList(Vertrag v)
        {
            if (v == null) return;

            AllRelevant = transform(include(WalterDbService)
                    .Where(b => b.Wohnungen.Exists(w => v.Wohnung?.WohnungId == w.WohnungId))
                    .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Wohnung w)
        {
            if (w == null) return;

            AllRelevant = transform(include(WalterDbService)
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
        private ImmutableList<UmlageListViewModelEntry> transform(List<Umlage> list)
        {
            return list
                .Select(w => new UmlageListViewModelEntry(w))
                .ToImmutableList();
        }
    }
}
