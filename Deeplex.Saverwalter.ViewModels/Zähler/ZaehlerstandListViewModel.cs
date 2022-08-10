using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    // TODO this is not a regular ListViewModel, since Zaehlerstaende needs a Zaehler
    public class ZaehlerstandListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<ZaehlerstandListViewModelEntry>> Liste = new();
        public int ZaehlerId { get; private set; }

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }
        public RelayCommand Add { get; }

        public void SetList(Zaehler z)
        {
            ZaehlerId = z.ZaehlerId;
            Liste.Value = z.Staende
                .Select(s => new ZaehlerstandListViewModelEntry(s, this))
                .OrderByDescending(e => e.Datum.Value)
                .ToImmutableList();
        }

        public ZaehlerstandListViewModel(Zaehler z, INotificationService ns, IWalterDbService db)
        {
            NotificationService = ns;
            WalterDbService = db;

            Add = new RelayCommand(_ =>
            {
                var zs = new Zaehlerstand
                {
                    Datum = System.DateTime.Today.AsUtcKind(),
                    Zaehler = z
                };
                Liste.Value = Liste.Value.Prepend(new ZaehlerstandListViewModelEntry(zs, this)).ToImmutableList();
            }, _ => true);
        }

    }
}
