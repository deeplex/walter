using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietminderungListViewModelEntry : ListViewModelEntry<Mietminderung>, IDetailViewModel
    {
        public override string ToString() => (Minderung.Value * 100).ToString() + "€";

        public Mietminderung Entity { get; }

        public SavableProperty<DateTimeOffset> Beginn { get; }
        public SavableProperty<DateTimeOffset?> Ende { get; }
        public SavableProperty<double> Minderung { get; }
        public SavableProperty<string> Notiz { get; }

        public INotificationService NotificationService { get; }
        public IWalterDbService WalterDbService { get; }
        public AsyncRelayCommand Delete { get; }
        public RelayCommand Save { get; }

        public MietminderungListViewModelEntry(Mietminderung m, MietminderungListViewModel vm)
        {
            Entity = m;
            WalterDbService = vm.WalterDbService;
            NotificationService = vm.NotificationService;

            Beginn = new(this, m.Beginn);
            Ende = new(this, m.Ende);
            Minderung = new(this, m.Minderung);
            Notiz = new(this, m.Notiz);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (Entity.MietminderungId != 0 && await vm.NotificationService.Confirmation())
                {
                    vm.WalterDbService.ctx.Mietminderungen.Remove(Entity);
                    vm.WalterDbService.SaveWalter();
                }
                vm.Liste.Value = vm.Liste.Value.Remove(this);
            }, _ => true);
        }

        public void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Beginn != Beginn.Value.UtcDateTime ||
                Entity.Ende != Ende.Value?.UtcDateTime ||
                Entity.Notiz != Notiz.Value ||
                Entity.Minderung != Minderung.Value;
        }

        public void save()
        {
            Entity.Beginn = Beginn.Value.UtcDateTime;
            Entity.Ende = Ende.Value?.UtcDateTime;
            Entity.Notiz = Notiz.Value;
            Entity.Minderung = Minderung.Value;

            if (Entity.MietminderungId != 0)
            {
                WalterDbService.ctx.Mietminderungen.Update(Entity);
            }
            else
            {
                WalterDbService.ctx.Mietminderungen.Add(Entity);
            }
            WalterDbService.SaveWalter();
            checkForChanges();
        }
    }
}
