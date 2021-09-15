using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerstandListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<ZaehlerstandListEntry>> Liste = new ObservableProperty<ImmutableList<ZaehlerstandListEntry>>();
        public int ZaehlerId;

        public IAppImplementation Impl;

        public ZaehlerstandListViewModel(Zaehler z, IAppImplementation impl)
        {
            ZaehlerId = z.ZaehlerId;
            Impl = impl;
            Liste.Value = z.Staende.Select(s => new ZaehlerstandListEntry(s, this)).ToImmutableList();
        }

        public void AddToList(Zaehlerstand z)
        {
            Liste.Value = Liste.Value.Add(new ZaehlerstandListEntry(z, this));
        }
    }

    public class ZaehlerstandListEntry
    {
        public Zaehlerstand Entity { get; }
        public int Id => Entity.ZaehlerstandId;
        public double Stand => Entity.Stand;
        public string StandString => Entity.Stand.ToString();
        public DateTimeOffset Datum => Entity.Datum;
        public string DatumString => Datum.ToString("dd.MM.yyyy");

        public ZaehlerstandListEntry(Zaehlerstand z, ZaehlerstandListViewModel vm)
        {
            Entity = z;
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await vm.Impl.Confirmation())
                {
                    vm.Impl.ctx.Zaehlerstaende.Remove(Entity);
                    vm.Impl.SaveWalter();
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                }
            }, _ => true);
        }

        public AsyncRelayCommand Delete;
    }
}
