using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietenListViewModelEntry : BindableBase
    {
        public Miete Entity { get; }
        public double Betrag
        {
            get => Entity.Betrag ?? 0;
            set
            {
                var old = Entity.Betrag;
                Entity.Betrag = value;
                RaisePropertyChangedAuto(old, value);
            }
        }
        public DateTime Zahlungsdatum => Entity.Zahlungsdatum;
        public DateTimeOffset BetreffenderMonat
        {
            get => Entity.BetreffenderMonat;
            set
            {
                var old = Entity.BetreffenderMonat;
                Entity.BetreffenderMonat = value.UtcDateTime;
                RaisePropertyChangedAuto(old, value);
            }
        }
        public string Notiz
        {
            get => Entity.Notiz;
            set
            {
                var old = Entity.Notiz;
                Entity.Notiz = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        AppViewModel Avm;

        public MietenListViewModelEntry(Miete m, MietenListViewModel vm)
        {
            Entity = m;

            Avm = vm.Avm;

            SelfDestruct = new AsyncRelayCommand(async _ =>
            {
                if (await vm.Impl.Confirmation())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                    vm.Avm.ctx.Mieten.Remove(Entity);
                    vm.Avm.SaveWalter();
                }
            }, _ => true);

            PropertyChanged += OnUpdate;
        }

        private void OnUpdate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BetreffenderMonat):
                case nameof(Betrag):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if (Entity.VertragId == Guid.Empty)
            {
                return;
            }


            if (Entity.MieteId != 0)
            {
                Avm.ctx.Mieten.Update(Entity);
            }
            else
            {
                Avm.ctx.Mieten.Add(Entity);
            }
            Avm.SaveWalter();
        }

        public AsyncRelayCommand SelfDestruct;
    }
}
