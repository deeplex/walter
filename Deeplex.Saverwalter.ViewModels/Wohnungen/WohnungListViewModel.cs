using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungListViewModel : ListViewModel<WohnungListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Wohnungen";

        protected override void updateList()
        {
            List.Value = AllRelevant.Where(v => applyFilter(v.Bezeichnung, v.Anschrift)).ToImmutableList();
        }

        public WohnungListViewModel(INotificationService notificationService, IWalterDbService walterDbService)
        {
            NotificationService = notificationService;
            WalterDbService = walterDbService;

            Navigate = new RelayCommand(el => notificationService.Navigation((Wohnung)el), _ => true);
        }

        private List<Wohnung> include()
        {
            return WalterDbService.ctx.Wohnungen
               .Include(w => w.Anhaenge)
               .Include(w => w.Adresse)
               .ThenInclude(a => a.Anhaenge)
               .ToList();
        }

        public override void SetList()
        {
            AllRelevant = include().Select(e => new WohnungListViewModelEntry(e, WalterDbService));
            updateList();
        }

        public void SetList(IPerson e)
        {
            if (e == null) return;

            var vertragIds = WalterDbService.ctx.MieterSet
                .ToList()
                .Where(m => m.PersonId == e.PersonId)
                .Select(m => m.VertragId)
                .ToList();

            var wohnungen = WalterDbService.ctx.Vertraege
                .Include(v => v.Wohnung)
                .ToList()
                .Where(v =>
                    vertragIds.Exists(i => v.VertragId == i) ||
                    v.Wohnung.BesitzerId == e.PersonId ||
                    v.AnsprechpartnerId == e.PersonId)
                .Select(v => v.Wohnung)
                .ToList();

            AllRelevant = include()
                .Where(w => wohnungen
                    .Exists(e => e.WohnungId == w.WohnungId))
                    .Select(e => new WohnungListViewModelEntry(e, WalterDbService));
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Umlage e)
        {
            if (e == null) return;

            AllRelevant = include()
                .Where(w => w.Umlagen.Exists(b => b.UmlageId == e.UmlageId))
                .Select(e => new WohnungListViewModelEntry(e, WalterDbService));
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Adresse e)
        {
            if (e == null) return;

            AllRelevant = include()
                .Where(w => w.AdresseId == e.AdresseId)
                .Select(e => new WohnungListViewModelEntry(e, WalterDbService));
            List.Value = AllRelevant.ToImmutableList();
        }
    }
}
