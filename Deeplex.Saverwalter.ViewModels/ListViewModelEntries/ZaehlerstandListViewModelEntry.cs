using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerstandListViewModelEntry : DetailViewModel<Zaehlerstand>, IDetailViewModel
    {
        public override string ToString() => Stand.ToString();

        public SavableProperty<double> Stand { get; private set; }
        public SavableProperty<DateTimeOffset> Datum { get; private set; }
        public SavableProperty<string> Notiz { get; private set; }

        public ZaehlerstandListViewModelEntry(Zaehlerstand z, ZaehlerstandListViewModel vm): base(vm.NotificationService, vm.WalterDbService)
        {
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await delete())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                };
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);
            
            SetEntity(z);
        }

        public override void SetEntity(Zaehlerstand z)
        {
            Entity = z;
            Stand = new(this, z.Stand);
            Datum = new(this, z.Datum);
            Notiz = new(this, z.Notiz);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Stand != Stand.Value ||
                Entity.Datum.AsUtcKind() != Datum.Value ||
                Entity.Notiz != Notiz.Value;
        }

        private new void save()
        {
            Entity.Stand = Stand.Value;
            Entity.Datum = Datum.Value.UtcDateTime;
            Entity.Notiz = Notiz.Value;

            base.save();
        }
    }
}
