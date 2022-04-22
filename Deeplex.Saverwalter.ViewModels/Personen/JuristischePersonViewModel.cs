using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
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
            if (await Impl.Confirmation())
            {
                Avm.ctx.JuristischePersonen.Remove(GetEntity);
                Avm.SaveWalter();
            }
        }

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Mitglieder
            = new ObservableProperty<ImmutableList<KontaktListViewModelEntry>>();
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> AddMitglieder
            = new ObservableProperty<ImmutableList<KontaktListViewModelEntry>>();
        public ObservableProperty<KontaktListViewModelEntry> AddMitglied
            = new ObservableProperty<KontaktListViewModelEntry>();

        public bool WohnungenInklusiveMitglieder
        {
            get => mInklusiveZusatz;
            set
            {
                mInklusiveZusatz = value;
                UpdateListen();
            }
        }

        public void UpdateListen()
        {
            Mitglieder.Value = Avm.ctx.JuristischePersonenMitglieder
                .Where(w => w.JuristischePersonId == Id)
                .Select(w => new KontaktListViewModelEntry(w.PersonId, Avm))
                .ToImmutableList();

            AddMitglieder.Value = Avm.ctx.NatuerlichePersonen
                .Select(k => new KontaktListViewModelEntry(k))
                .ToList()
                .Concat(Avm.ctx.JuristischePersonen
                    .Select(k => new KontaktListViewModelEntry(k))
                    .ToList())
                .Where(k => !Mitglieder.Value.Any(e => e.Guid == k.Guid))
                    .ToImmutableList();

            Wohnungen.Value = Avm.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == GetEntity.PersonId ||
                    (WohnungenInklusiveMitglieder && Mitglieder.Value.Any(m => m.Guid == w.BesitzerId)))
                .Select(w => new WohnungListViewModelEntry(w, Avm))
                .ToImmutableList();
        }

        public RelayCommand AddMitgliedCommand;

        public JuristischePersonViewModel(IAppImplementation impl, AppViewModel avm) : this(new JuristischePerson(), impl, avm) { }
        public JuristischePersonViewModel(int id, IAppImplementation impl, AppViewModel avm) : this(avm.ctx.JuristischePersonen.Find(id), impl, avm) { }
        public JuristischePersonViewModel(JuristischePerson j, IAppImplementation impl, AppViewModel avm) : base(impl, avm)
        {
            Entity = j;
            Id = j.JuristischePersonId;

            UpdateListen();

            PropertyChanged += OnUpdate;
            AddMitgliedCommand = new RelayCommand(_ =>
            {
                if (AddMitglied.Value?.Guid is Guid guid)
                {
                    Avm.ctx.JuristischePersonenMitglieder.Add(new JuristischePersonenMitglied()
                    {
                        JuristischePersonId = Id,
                        PersonId = AddMitglied.Value.Guid,
                    });
                    Avm.SaveWalter();
                    UpdateListen();
                }
            }, _ => true);
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
                Avm.ctx.JuristischePersonen.Update(GetEntity);
            }
            else
            {
                Avm.ctx.JuristischePersonen.Add(GetEntity);
            }
            Avm.SaveWalter();
        }
    }
}
