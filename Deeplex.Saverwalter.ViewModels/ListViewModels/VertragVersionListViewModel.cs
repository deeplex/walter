using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragVersionListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<VertragVersionListViewModelEntry>> Liste = new();

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }
        public RelayCommand Add { get; }

        public Vertrag Entity { get; private set; }

        public void SetList(Vertrag v)
        {
            Entity = v;

            Liste.Value = v.Versionen
                .Select(s => new VertragVersionListViewModelEntry(s, WalterDbService, NotificationService))
                .OrderByDescending(e => e.Entity.Beginn)
                .ToImmutableList();
        }

        public VertragVersionListViewModel(INotificationService ns, IWalterDbService db)
        {
            NotificationService = ns;
            WalterDbService = db;

            Add = new RelayCommand(_ =>
            {
                var vv = new VertragVersion
                {
                    // TODO select values smarter
                    Grundmiete = 500,
                    Personenzahl = 2,
                    Beginn = System.DateTime.Now.AsUtcKind(),
                    Vertrag = Entity,
                };
                Liste.Value = Liste.Value.Prepend(
                    new VertragVersionListViewModelEntry(vv, WalterDbService, NotificationService))
                .ToImmutableList();
            }, _ => true);
        }

    }
}
