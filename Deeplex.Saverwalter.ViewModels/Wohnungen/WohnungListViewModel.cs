using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungListViewModel : ListViewModel<WohnungListViewModelEntry>, IListViewModel<WohnungListViewModelEntry>
    {
        public override string ToString() => "Wohnungen";

        protected override ImmutableList<WohnungListViewModelEntry> updateList(string filter = "")
            => AllRelevant.Where(v => applyFilter(filter, v.Bezeichnung, v.Anschrift)).ToImmutableList();

        public INotificationService NotificationService { get; }
        public IWalterDbService WalterDbService { get; }

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

        public void SetList()
        {
            AllRelevant = include()
                .Select(e => new WohnungListViewModelEntry(e, WalterDbService));
            updateList();
        }

        public void SetList(IPerson e)
        {
            var vertragIds = WalterDbService.ctx.MieterSet
                .ToList()
                .Where(m => m.PersonId == e.PersonId)
                .Select(m => m.VertragId)
                .ToList();

            var wohnungen = WalterDbService.ctx.Vertraege
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
            updateList();
        }

        public void SetList(Umlage e)
        {
            AllRelevant = include()
                .Where(w => w.Umlagen.Exists(b => b.UmlageId == e.UmlageId))
                .Select(e => new WohnungListViewModelEntry(e, WalterDbService));
            updateList();
        }

        public void SetList(Adresse e)
        {
            AllRelevant = include()
                .Where(w => w.AdresseId == e.AdresseId)
                .Select(e => new WohnungListViewModelEntry(e, WalterDbService));
            updateList();
        }
    }
}
