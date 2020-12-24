using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.ComponentModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class JuristischePersonViewModel : BindableBase
    {
        private JuristischePerson Entity { get; }
        public JuristischePerson GetEntity => Entity;
        public int Id;

        public void selfDestruct()
        {
            App.Walter.JuristischePersonen.Remove(Entity);
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

        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set
            {
                Entity.Bezeichnung = value;
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

        public override string ToString() => Bezeichnung;

        public JuristischePersonViewModel() : this(new JuristischePerson()) { IsInEdit.Value = true; }
        public JuristischePersonViewModel(int id) : this(App.Walter.JuristischePersonen.Find(id)) { }
        public JuristischePersonViewModel(JuristischePerson j)
        {
            Entity = j;
            Id = j.JuristischePersonId;

            PropertyChanged += OnUpdate;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.JuristischePersonAnhaenge, j), _ => true);
        }

        public AsyncRelayCommand AttachFile;

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Bezeichnung):
                case nameof(Email):
                case nameof(Telefon):
                case nameof(Mobil):
                case nameof(Fax):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if (Entity.Bezeichnung == null)
            {
                return;
            }

            if (Entity.JuristischePersonId != 0)
            {
                App.Walter.JuristischePersonen.Update(Entity);
            }
            else
            {
                App.Walter.JuristischePersonen.Add(Entity);
            }
            App.SaveWalter();
        }
    }
}
