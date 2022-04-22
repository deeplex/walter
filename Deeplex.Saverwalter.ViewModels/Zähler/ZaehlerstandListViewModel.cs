using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerstandListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<ZaehlerstandListViewModelEntry>> Liste = new ObservableProperty<ImmutableList<ZaehlerstandListViewModelEntry>>();
        public int ZaehlerId;

        public AppViewModel Avm;
        public IAppImplementation Impl;

        public ZaehlerstandListViewModel(Zaehler z, IAppImplementation impl, AppViewModel avm)
        {
            ZaehlerId = z.ZaehlerId;
            Impl = impl;
            Avm = avm;
            Liste.Value = z.Staende.Select(s => new ZaehlerstandListViewModelEntry(s, this)).ToImmutableList();
        }

        public void AddToList(Zaehlerstand z)
        {
            Liste.Value = Liste.Value.Add(new ZaehlerstandListViewModelEntry(z, this));
        }
    }
}
