using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class NatuerlichePersonViewModel : BindableBase
    {
        public NatuerlichePerson Entity { get; }
        public int Id { get; }

        public List<Anrede> Anreden { get; }

        public void selfDestruct()
        {
            App.Walter.NatuerlichePersonen.Remove(Entity);
            App.SaveWalter();
        }

        public Guid PersonId
        {
            get => Entity.PersonId;
            set
            {
                Entity.PersonId = value;
                RaisePropertyChangedAuto();
            }
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
            get => Entity.Vorname;
            set
            {
                Entity.Vorname = value;
                RaisePropertyChangedAuto();
            }
        }
        public string Nachname
        {
            get => Entity.Nachname;
            set
            {
                Entity.Nachname = value;
                RaisePropertyChangedAuto();
            }
        }
        public bool isVermieter
        {
            get => Entity.isVermieter;
            set
            {
                Entity.isVermieter = value;
                RaisePropertyChangedAuto();
            }
        }
        public bool isMieter
        {
            get => Entity.isMieter;
            set
            {
                Entity.isMieter = value;
                RaisePropertyChangedAuto();
            }
        }

        public bool isHandwerker
        {
            get => Entity.isHandwerker;
            set
            {
                Entity.isHandwerker = value;
                RaisePropertyChangedAuto();
            }
        }

        public int AdresseId => Entity.AdresseId ?? 0;


        public string Email
        {
            get => Entity.Email;
            set
            {
                Entity.Email = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Telefon
        {
            get => Entity.Telefon;
            set
            {
                Entity.Telefon = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Mobil
        {
            get => Entity.Mobil;
            set
            {
                Entity.Mobil = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Fax
        {
            get => Entity.Fax;
            set
            {
                Entity.Fax = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Notiz
        {
            get => Entity.Notiz;
            set
            {
                Entity.Notiz = value;
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

        public AsyncRelayCommand AttachFile;
        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

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

            if (Entity.Nachname == null)
            {
                return;
            }

            if (Entity.NatuerlichePersonId != 0)
            {
                App.Walter.NatuerlichePersonen.Update(Entity);
            }
            else
            {
                App.Walter.NatuerlichePersonen.Add(Entity);
            }
            App.SaveWalter();
        }
    }
}
