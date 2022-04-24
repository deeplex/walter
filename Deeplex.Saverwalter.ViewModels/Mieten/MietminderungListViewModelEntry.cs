using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietminderungListViewModelEntry
    {
        public MietMinderung Entity { get; }

        public DateTime Beginn => Entity.Beginn;
        public DateTime? Ende => Entity.Ende;
        public double Minderung => Entity.Minderung;
        public string Notiz => Entity.Notiz;

        public MietminderungListViewModelEntry(MietMinderung m, MietMinderungListViewModel vm)
        {
            Entity = m;

            SelfDestruct = new AsyncRelayCommand(async _ =>
            {
                if (await vm.NotificationService.Confirmation())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                    vm.Db.ctx.MietMinderungen.Remove(Entity);
                    vm.Db.SaveWalter();
                }

            }, _ => true);
        }
        public AsyncRelayCommand SelfDestruct;
    }
}
