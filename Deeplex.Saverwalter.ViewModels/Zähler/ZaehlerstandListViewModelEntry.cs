using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerstandListViewModelEntry : IDetail
    {
        public Zaehlerstand Entity { get; }
        public int Id => Entity.ZaehlerstandId;
        public SavableProperty<double> Stand { get; }
        public SavableProperty<DateTimeOffset> Datum { get; }
        public SavableProperty<string> Notiz { get; }

        private IWalterDbService Db { get; }
        private INotificationService NotificationService { get; }

        public ZaehlerstandListViewModelEntry(Zaehlerstand z, ZaehlerstandListViewModel vm)
        {
            Db = vm.Db;
            NotificationService = vm.NotificationService;
            Entity = z;
            Stand = new(this, z.Stand);
            Datum = new(this, z.Datum);
            Notiz = new(this, z.Notiz);
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (Entity.ZaehlerstandId != 0 && await vm.NotificationService.Confirmation())
                {
                    vm.Db.ctx.Zaehlerstaende.Remove(Entity);
                    vm.Db.SaveWalter();
                }
                vm.Liste.Value = vm.Liste.Value.Remove(this);
            }, _ => true);
            Save = new RelayCommand(_ => save(), _ => true);
        }

        public void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Stand != Stand.Value ||
                Entity.Datum.AsUtcKind() != Datum.Value ||
                Entity.Notiz != Notiz.Value;
        }

        private void save()
        {
            Entity.Stand = Stand.Value;
            Entity.Datum = Datum.Value.UtcDateTime;
            Entity.Notiz = Notiz.Value;

            if (Entity.ZaehlerstandId != 0)
            {
                Db.ctx.Zaehlerstaende.Update(Entity);
            }
            else
            {
                Db.ctx.Zaehlerstaende.Add(Entity);
            }
            Db.SaveWalter();
            checkForChanges();
        }

        public AsyncRelayCommand Delete { get; }
        public RelayCommand Save { get; }
    }
}
