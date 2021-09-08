﻿using Deeplex.Saverwalter.Model;
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

        public IAppImplementation Impl;

        public MietenListViewModel(Guid VertragGuid, IAppImplementation impl)
        {
            VertragId = VertragGuid;
            Impl = impl;
            var self = this;
            Liste.Value = Impl.ctx.Mieten
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

        IAppImplementation Impl;

        public MietenListEntry(Miete m, MietenListViewModel vm)
        {
            Entity = m;

            Impl = vm.Impl;

            SelfDestruct = new AsyncRelayCommand(async _ =>
            {
                if (await App.ViewModel.Confirmation())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                    App.Walter.Mieten.Remove(Entity);
                    App.SaveWalter();
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
                Impl.ctx.Mieten.Update(Entity);
            }
            else
            {
                Impl.ctx.Mieten.Add(Entity);
            }
            Impl.SaveWalter();
        }

        public AsyncRelayCommand SelfDestruct;
    }
}