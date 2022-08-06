using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietminderungListViewModelEntry : IDetailViewModel
    {
        public MietMinderung Entity { get; }

        public SavableProperty<DateTimeOffset> Beginn { get; }
        public SavableProperty<DateTimeOffset?> Ende { get; }
        public SavableProperty<double> Minderung { get; }
        public SavableProperty<string> Notiz { get; }

        private INotificationService NotificationService { get; }
        private IWalterDbService Db { get; }
        public AsyncRelayCommand Delete { get; }
        public RelayCommand Save { get; }

        public MietminderungListViewModelEntry(MietMinderung m, MietMinderungListViewModel vm)
        {
            Entity = m;
            Db = vm.Db;
            NotificationService = vm.NotificationService;

            Beginn = new(this, m.Beginn);
            Ende = new(this, m.Ende);
            Minderung = new(this, m.Minderung);
            Notiz = new(this, m.Notiz);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (Entity.MietMinderungId != 0 && await vm.NotificationService.Confirmation())
                {
                    vm.Db.ctx.MietMinderungen.Remove(Entity);
                    vm.Db.SaveWalter();
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

            if (Entity.MietMinderungId != 0)
            {
                Db.ctx.MietMinderungen.Update(Entity);
            }
            else
            {
                Db.ctx.MietMinderungen.Add(Entity);
            }
            Db.SaveWalter();
            checkForChanges();
        }
    }
}
