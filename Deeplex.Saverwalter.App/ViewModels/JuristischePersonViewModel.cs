using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class JuristischePersonViewModel : BindableBase
    {
        private JuristischePerson Entity { get; }
        public int Id;

        public ImmutableList<AdresseViewModel> AlleAdressen { get; }
        public ImmutableList<Anrede> Anreden { get; }

        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set
            {
                Entity.Bezeichnung = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Name
        {
            get => Entity.Bezeichnung;
            set
            {
                Entity.Bezeichnung = value;
                RaisePropertyChangedAuto();
            }
        }
        private AdresseViewModel mAdresse;
        public AdresseViewModel Adresse
        {
            get => mAdresse;
            set
            {
                Entity.AdresseId = value.Id;
                mAdresse = value;
                RaisePropertyChangedAuto();
            }
        }

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

        public override string ToString() => Name;

        public JuristischePersonViewModel() : this(new JuristischePerson()) { IsInEdit.Value = true; }
        public JuristischePersonViewModel(int id) : this(App.Walter.JuristischePersonen.Find(id)) { }
        public JuristischePersonViewModel(JuristischePerson j)
        {
            Entity = j;
            Id = j.JuristischePersonId;
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Bezeichnung):
                case nameof(Adresse):
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
            App.Walter.SaveChanges();
        }

        public static JuristischePerson GetJuristischePerson(JuristischePersonViewModel j)
        {
            if (j.Id != 0)
            {
                return j.Entity;
            }
            else // TODO where is this applicable? ...
            {
                return App.Walter.JuristischePersonen.First(p => p.Bezeichnung == j.Name);
            }
        }
    }
}
