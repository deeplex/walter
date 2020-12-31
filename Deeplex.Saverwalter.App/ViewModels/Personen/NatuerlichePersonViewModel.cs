﻿using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class NatuerlichePersonViewModel : PersonViewModel
    {
        public NatuerlichePerson GetEntity => (NatuerlichePerson)Entity;

        public int Id { get; }

        public List<Anrede> Anreden { get; }

        public void selfDestruct()
        {
            App.Walter.NatuerlichePersonen.Remove(GetEntity);
            App.SaveWalter();
        }

        public Anrede Anrede
        {
            get => Entity.Anrede;
            set
            {
                Entity.Anrede = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Vorname
        {
            get => GetEntity.Vorname;
            set
            {
                GetEntity.Vorname = value;
                RaisePropertyChangedAuto();
            }
        }
        public string Nachname
        {
            get => GetEntity.Nachname;
            set
            {
                GetEntity.Nachname = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Name => Vorname + " " + Nachname;

        public NatuerlichePersonViewModel(int id)
            : this(App.Walter.NatuerlichePersonen.Find(id)) { }

        public NatuerlichePersonViewModel() : this(new NatuerlichePerson()) { IsInEdit.Value = true; }
        public NatuerlichePersonViewModel(NatuerlichePerson k)
        {
            Entity = k;
            Id = k.NatuerlichePersonId;

            Anreden = Enums.Anreden;

            PropertyChanged += OnUpdate;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.NatuerlichePersonAnhaenge, k), _ => true);
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
                    break;
                default:
                    return;
            }

            if (GetEntity.Nachname == null)
            {
                return;
            }

            if (GetEntity.NatuerlichePersonId != 0)
            {
                App.Walter.NatuerlichePersonen.Update(GetEntity);
            }
            else
            {
                App.Walter.NatuerlichePersonen.Add(GetEntity);
            }
            App.SaveWalter();
        }
    }
}
