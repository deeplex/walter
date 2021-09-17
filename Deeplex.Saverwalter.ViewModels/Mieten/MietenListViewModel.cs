using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietenListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<MietenListEntry>> Liste
            = new ObservableProperty<ImmutableList<MietenListEntry>>();
        public Guid VertragId;

        public AppViewModel Avm;
        public IAppImplementation Impl;

        public MietenListViewModel(Guid VertragGuid, IAppImplementation impl, AppViewModel avm)
        {
            VertragId = VertragGuid;
            Avm = avm;
            Impl = impl;
            var self = this;
            Liste.Value = Avm.ctx.Mieten
                .Where(m => m.VertragId == VertragGuid)
                .Select(m => new MietenListEntry(m, self))
                .ToImmutableList();
        }

        public void AddToList(Miete z)
        {
            Liste.Value = Liste.Value.Add(new MietenListEntry(z, this));
        }
    }

    public sealed class MietenListEntry : BindableBase
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
        public string DatumString => Entity.Zahlungsdatum.ToString("dd.MM.yyyy");
        public DateTimeOffset Monat
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

        public MietenListEntry(Miete m, MietenListViewModel vm)
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
                case nameof(Monat):
                case nameof(Betrag):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if (Monat == null || Entity.VertragId == Guid.Empty || Entity.VertragId == null)
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
