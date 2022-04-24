using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.Services.WalterDbService;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerstandListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<ZaehlerstandListViewModelEntry>> Liste = new ObservableProperty<ImmutableList<ZaehlerstandListViewModelEntry>>();
        public int ZaehlerId;

        public Services.IWalterDbService Db;
        public IAppImplementation Impl;

        public ZaehlerstandListViewModel(Zaehler z, IAppImplementation impl, Services.IWalterDbService db)
        {
            ZaehlerId = z.ZaehlerId;
            Impl = impl;
            Db = db;
            Liste.Value = z.Staende.Select(s => new ZaehlerstandListViewModelEntry(s, this)).ToImmutableList();
        }

        public void AddToList(Zaehlerstand z)
        {
            Liste.Value = Liste.Value.Add(new ZaehlerstandListViewModelEntry(z, this));
        }
    }
}
