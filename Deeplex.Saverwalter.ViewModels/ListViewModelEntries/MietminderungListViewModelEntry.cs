using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietminderungListViewModelEntry : DetailViewModel<Mietminderung>, IDetailViewModel
    {
        public override string ToString() => (Minderung.Value * 100).ToString() + "€";

        public SavableProperty<DateTimeOffset> Beginn { get; private set; }
        public SavableProperty<DateTimeOffset?> Ende { get; private set; }
        public SavableProperty<double> Minderung { get; private set; }
        public SavableProperty<string> Notiz { get; private set; }

        public MietminderungListViewModelEntry(Mietminderung m, MietminderungListViewModel vm) : base(vm.NotificationService, vm.WalterDbService)
        {
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await delete())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                };
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public override void SetEntity(Mietminderung m)
        {
            Entity = m;

            Beginn = new(this, m.Beginn);
            Ende = new(this, m.Ende);
            Minderung = new(this, m.Minderung);
            Notiz = new(this, m.Notiz);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Beginn != Beginn.Value.UtcDateTime ||
                Entity.Ende != Ende.Value?.UtcDateTime ||
                Entity.Notiz != Notiz.Value ||
                Entity.Minderung != Minderung.Value;
        }

        public new void save()
        {
            Entity.Beginn = Beginn.Value.UtcDateTime;
            Entity.Ende = Ende.Value?.UtcDateTime;
            Entity.Notiz = Notiz.Value;
            Entity.Minderung = Minderung.Value;

            base.save();
        }
    }
}
