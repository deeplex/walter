using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerstandListViewModelEntry
    {
        public Zaehlerstand Entity { get; }
        public int Id => Entity.ZaehlerstandId;
        public double Stand => Entity.Stand;
        public string StandString => Entity.Stand.ToString();
        public DateTimeOffset Datum => Entity.Datum;
        public string DatumString => Datum.ToString("dd.MM.yyyy");

        // TODO reverse injection?
        public ZaehlerstandListViewModelEntry(Zaehlerstand z, ZaehlerstandListViewModel vm)
        {
            Entity = z;
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await vm.Impl.Confirmation())
                {
                    vm.Db.ctx.Zaehlerstaende.Remove(Entity);
                    vm.Db.SaveWalter();
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                }
            }, _ => true);
        }

        public AsyncRelayCommand Delete;
    }
}
