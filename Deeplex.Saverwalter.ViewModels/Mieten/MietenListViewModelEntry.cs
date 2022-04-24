using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
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

        IWalterDbService Db;

        public MietenListViewModelEntry(Miete m, MietenListViewModel vm)
        {
            Entity = m;

            Db = vm.Db;

            SelfDestruct = new AsyncRelayCommand(async _ =>
            {
                if (await vm.NotificationService.Confirmation())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                    vm.Db.ctx.Mieten.Remove(Entity);
                    vm.Db.SaveWalter();
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
                Db.ctx.Mieten.Update(Entity);
            }
            else
            {
                Db.ctx.Mieten.Add(Entity);
            }
            Db.SaveWalter();
        }

        public AsyncRelayCommand SelfDestruct;
    }
}
