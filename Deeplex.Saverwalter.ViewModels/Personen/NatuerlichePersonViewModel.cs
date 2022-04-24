﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class NatuerlichePersonViewModel : PersonViewModel
    {
        public new NatuerlichePerson Entity => (NatuerlichePerson)base.Entity;

        public int Id { get; }

        public List<Anrede> Anreden { get; }

        public async void selfDestruct()
        {
            if (await Impl.Confirmation())
            {
                Avm.ctx.NatuerlichePersonen.Remove(Entity);
                Avm.SaveWalter();
            }
        }

        public Anrede Anrede
        {
            get => base.Entity.Anrede;
            set
            {
                var old = base.Entity.Anrede;
                base.Entity.Anrede = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Vorname
        {
            get => Entity.Vorname;
            set
            {
                var old = Entity.Vorname;
                Entity.Vorname = value;
                RaisePropertyChangedAuto(old, value);
            }
        }
        public string Nachname
        {
            get => Entity.Nachname;
            set
            {
                var old = Entity.Nachname;
                Entity.Nachname = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Name => Vorname + " " + Nachname;

        public bool WohnungenInklusiveJurPers
        {
            get => mInklusiveZusatz;
            set
            {
                mInklusiveZusatz = value;
                UpdateListen();
            }
        }

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> JuristischePersonen
            = new ObservableProperty<ImmutableList<KontaktListViewModelEntry>>();

        public void UpdateListen()
        {
            JuristischePersonen.Value = Avm.ctx.JuristischePersonenMitglieder
                .Include(w => w.JuristischePerson)
                .Where(w => w.PersonId == Entity.PersonId)
                .Select(w => new KontaktListViewModelEntry(w.JuristischePerson.PersonId, Avm))
                .ToImmutableList();

            Wohnungen.Value = Avm.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == Entity.PersonId ||
                    (WohnungenInklusiveJurPers && JuristischePersonen.Value.Any(m => m.Entity.PersonId == w.BesitzerId)))
                .Select(w => new WohnungListViewModelEntry(w, Avm))
                .ToImmutableList();
        }

        public NatuerlichePersonViewModel(int id, IAppImplementation impl, IWalterDbService db) : this(db.ctx.NatuerlichePersonen.Find(id), impl, db) { }
        public NatuerlichePersonViewModel(IAppImplementation impl, IWalterDbService db) : this(new NatuerlichePerson(), impl, db) { }
        public NatuerlichePersonViewModel(NatuerlichePerson k, IAppImplementation impl, IWalterDbService db) : base(impl, db)
        {
            base.Entity = k;
            Id = k.NatuerlichePersonId;

            Anreden = Enums.Anreden;
            UpdateListen();

            PropertyChanged += OnUpdate;
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Anrede):
                case nameof(Vorname):
                case nameof(Nachname):
                case nameof(Email):
                case nameof(Telefon):
                case nameof(Mobil):
                case nameof(Fax):
                case nameof(Notiz):
                case nameof(isHandwerker):
                case nameof(isMieter):
                case nameof(isVermieter):
                    break;
                default:
                    return;
            }

            if (Entity.Nachname == null)
            {
                return;
            }

            if (Entity.NatuerlichePersonId != 0)
            {
                Avm.ctx.NatuerlichePersonen.Update(Entity);
            }
            else
            {
                Avm.ctx.NatuerlichePersonen.Add(Entity);
            }
            Avm.SaveWalter();
        }
    }
}
