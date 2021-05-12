using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

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
                var old = GetEntity.Bezeichnung;
                GetEntity.Bezeichnung = value;
                RaisePropertyChangedAuto(old, value);
            }
        }
        public override string ToString() => Bezeichnung;

        public async void selfDestruct()
        {
            App.Walter.JuristischePersonen.Remove(GetEntity);
            App.SaveWalter();
        }

        public ObservableProperty<ImmutableList<KontaktListEntry>> Mitglieder
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();
        public ObservableProperty<ImmutableList<KontaktListEntry>> AddMitglieder
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();
        public ObservableProperty<KontaktListEntry> AddMitglied
            = new ObservableProperty<KontaktListEntry>();

        public void UpdateMitgliedList()
        {
            Mitglieder.Value = App.Walter.JuristischePersonenMitglieder
                .Where(w => w.JuristischePersonId == Id)
                .Select(w => new KontaktListEntry(w.PersonId))
                .ToImmutableList();

            AddMitglieder.Value = App.Walter.NatuerlichePersonen
                .Select(k => new KontaktListEntry(k))
                .ToList()
                .Concat(App.Walter.JuristischePersonen
                    .Select(k => new KontaktListEntry(k))
                    .ToList())
                .Where(k => !Mitglieder.Value.Any(e => e.Guid == k.Guid))
                    .ToImmutableList();
        }

        public JuristischePersonViewModel() : this(new JuristischePerson()) { }
        public JuristischePersonViewModel(int id) : this(App.Walter.JuristischePersonen.Find(id)) { }
        public JuristischePersonViewModel(JuristischePerson j)
        {
            Entity = j;
            Id = j.JuristischePersonId;

            UpdateMitgliedList();

            PropertyChanged += OnUpdate;
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
                case nameof(isHandwerker):
                case nameof(isMieter):
                case nameof(isVermieter):
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
