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

        private void update<T>(string property, T value)
        {
            if (Entity == null) return;
            var type = Entity.GetType();
            var prop = type.GetProperty(property);
            var val = prop.GetValue(Entity, null);
            if (!value.Equals(val))
            {
                prop.SetValue(Entity, value);
                RaisePropertyChanged(property);
            };
        }

        public Guid PersonId
        {
            get => Entity.PersonId;
            set => update(nameof(Entity.PersonId), value);
        }

        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set => update(nameof(Entity.Bezeichnung), value);

        }

        public bool isVermieter
        {
            get => Entity.isVermieter;
            set => update(nameof(Entity.isVermieter), value);

        }
        public bool isMieter
        {
            get => Entity.isMieter;
            set => update(nameof(Entity.isMieter), value);

        }
        public bool isHandwerker
        {
            get => Entity.isHandwerker;
            set => update(nameof(Entity.isHandwerker), value);

        }
        public int AdresseId => Entity.AdresseId ?? 0;

        public string Email
        {
            get => Entity.Email;
            set => update(nameof(Entity.Email), value);
        }

        public string Telefon
        {
            get => Entity.Telefon;
            set => update(nameof(Entity.Telefon), value);

        }

        public string Mobil
        {
            get => Entity.Mobil;
            set => update(nameof(Entity.Mobil), value);

        }

        public string Fax
        {
            get => Entity.Fax;
            set => update(nameof(Entity.Fax), value);

        }

        public string Notiz
        {
            get => Entity.Notiz;
            set => update(nameof(Entity.Notiz), value);
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
