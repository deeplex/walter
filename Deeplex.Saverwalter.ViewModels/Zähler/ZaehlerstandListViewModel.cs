using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerstandListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<ZaehlerstandListViewModelEntry>> Liste = new();
        public int ZaehlerId;

        public IWalterDbService Db;
        public INotificationService NotificationService;

        public ZaehlerstandListViewModel(Zaehler z, INotificationService ns, IWalterDbService db)
        {
            ZaehlerId = z.ZaehlerId;
            NotificationService = ns;
            Db = db;
            Liste.Value = z.Staende.Select(s => new ZaehlerstandListViewModelEntry(s, this)).ToImmutableList();
        }

        public void AddToList(Zaehlerstand z)
        {
            Liste.Value = Liste.Value.Add(new ZaehlerstandListViewModelEntry(z, this));
        }
    }
}
