using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.ComponentModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class JuristischePersonViewModel : PersonViewModel
    {
        public JuristischePerson GetEntity => (JuristischePerson)Entity;
        public int Id;

        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set
            {
                GetEntity.Bezeichnung = value;
                RaisePropertyChangedAuto();
            }
        }
        public override string ToString() => Bezeichnung;

        public void selfDestruct()
        {
            App.Walter.JuristischePersonen.Remove(GetEntity);
            App.SaveWalter();
        }

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

            if (GetEntity.JuristischePersonId != 0)
            {
                App.Walter.JuristischePersonen.Update(GetEntity);
            }
            else
            {
                App.Walter.JuristischePersonen.Add(GetEntity);
            }
            App.SaveWalter();
        }
    }
}
