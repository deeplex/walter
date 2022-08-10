using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietenListViewModelEntry : ListViewModelEntry<Miete>, DetailViewModel
    {
        public override string ToString() => Betrag + "€";

        public Miete Entity { get; }
        public SavableProperty<double> Betrag { get; }
        public SavableProperty<DateTimeOffset> Zahlungsdatum { get; }
        public SavableProperty<DateTimeOffset> BetreffenderMonat { get; }
        public SavableProperty<string> Notiz { get; }

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }
        public AsyncRelayCommand Delete { get; }
        public RelayCommand Save { get; }

        public MietenListViewModelEntry(Miete m, MietenListViewModel vm)
        {
            Entity = m;

            WalterDbService = vm.WalterDbService;
            NotificationService = vm.NotificationService;

            Betrag = new(this, m.Betrag ?? 0);
            Zahlungsdatum = new(this, m.Zahlungsdatum);
            BetreffenderMonat = new(this, m.BetreffenderMonat);
            Notiz = new(this, m.Notiz);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (Entity.MieteId != 0 && await vm.NotificationService.Confirmation())
                {
                    vm.WalterDbService.ctx.Mieten.Remove(Entity);
                    vm.WalterDbService.SaveWalter();
                }
                vm.Liste.Value = vm.Liste.Value.Remove(this);
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Betrag != Betrag.Value ||
                Entity.BetreffenderMonat.AsUtcKind() != BetreffenderMonat.Value ||
                Entity.Zahlungsdatum.AsUtcKind() != Zahlungsdatum.Value ||
                Entity.Notiz != Notiz.Value;
        }

        public void save()
        {
            Entity.Betrag = Betrag.Value;
            Entity.BetreffenderMonat = BetreffenderMonat.Value.UtcDateTime;
            Entity.Zahlungsdatum = Zahlungsdatum.Value.UtcDateTime;
            Entity.Notiz = Notiz.Value;

            if (Entity.MieteId != 0)
            {
                WalterDbService.ctx.Mieten.Update(Entity);
            }
            else
            {
                WalterDbService.ctx.Mieten.Add(Entity);
            }
            WalterDbService.SaveWalter();
            checkForChanges();
        }
    }
}
