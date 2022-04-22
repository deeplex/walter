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
                if (await vm.Impl.Confirmation())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                    vm.Avm.ctx.MietMinderungen.Remove(Entity);
                    vm.Avm.SaveWalter();
                }

            }, _ => true);
        }
        public AsyncRelayCommand SelfDestruct;
    }
}
