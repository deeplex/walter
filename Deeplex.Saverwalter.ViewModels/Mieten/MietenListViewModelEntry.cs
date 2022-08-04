using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietenListViewModelEntry : BindableBase, IDetail
    {
        public Miete Entity { get; }
        public SavableProperty<double> Betrag { get; }
        public SavableProperty<DateTimeOffset> Zahlungsdatum { get; }
        public SavableProperty<DateTimeOffset> BetreffenderMonat { get; }
        public SavableProperty<string> Notiz { get; }

        IWalterDbService Db;
        INotificationService NotificationService;
        public AsyncRelayCommand Delete { get; }
        public RelayCommand Save { get; }

        public MietenListViewModelEntry(Miete m, MietenListViewModel vm)
        {
            Entity = m;

            Db = vm.Db;
            NotificationService = vm.NotificationService;

            Betrag = new(this, m.Betrag ?? 0);
            Zahlungsdatum = new(this, m.Zahlungsdatum);
            BetreffenderMonat = new(this, m.BetreffenderMonat);
            Notiz = new(this, m.Notiz);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (Entity.MieteId != 0 && await vm.NotificationService.Confirmation())
                {
                    vm.Db.ctx.Mieten.Remove(Entity);
                    vm.Db.SaveWalter();
                }
                vm.Liste.Value = vm.Liste.Value.Remove(this);
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Betrag != Betrag.Value ||
                Entity.BetreffenderMonat != BetreffenderMonat.Value.UtcDateTime ||
                Entity.Zahlungsdatum != Zahlungsdatum.Value.UtcDateTime ||
                Entity.Notiz != Notiz.Value;
        }

        private void save()
        {
            Entity.Betrag = Betrag.Value;
            Entity.BetreffenderMonat = BetreffenderMonat.Value.UtcDateTime;
            Entity.Zahlungsdatum = Zahlungsdatum.Value.UtcDateTime;
            Entity.Notiz = Notiz.Value;

            if (Entity.MieteId != 0)
            {
                Db.ctx.Mieten.Update(Entity);
            }
            else
            {
                Db.ctx.Mieten.Add(Entity);
            }
            Db.SaveWalter();
            checkForChanges();
        }
    }
}
