using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
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
            Impl.ctx.JuristischePersonen.Remove(GetEntity);
            Impl.SaveWalter();
        }

        public ObservableProperty<ImmutableList<KontaktListEntry>> Mitglieder
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();
        public ObservableProperty<ImmutableList<KontaktListEntry>> AddMitglieder
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();
        public ObservableProperty<KontaktListEntry> AddMitglied
            = new ObservableProperty<KontaktListEntry>();

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
            Mitglieder.Value = Impl.ctx.JuristischePersonenMitglieder
                .Where(w => w.JuristischePersonId == Id)
                .Select(w => new KontaktListEntry(w.PersonId, Impl))
                .ToImmutableList();

            AddMitglieder.Value = Impl.ctx.NatuerlichePersonen
                .Select(k => new KontaktListEntry(k))
                .ToList()
                .Concat(Impl.ctx.JuristischePersonen
                    .Select(k => new KontaktListEntry(k))
                    .ToList())
                .Where(k => !Mitglieder.Value.Any(e => e.Guid == k.Guid))
                    .ToImmutableList();

            Wohnungen.Value = Impl.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == GetEntity.PersonId ||
                    (WohnungenInklusiveMitglieder && Mitglieder.Value.Any(m => m.Guid == w.BesitzerId)))
                .Select(w => new WohnungListEntry(w))
                .ToImmutableList();
        }

        private IAppImplementation Impl;

        public JuristischePersonViewModel(IAppImplementation impl) : this(new JuristischePerson(), impl) { }
        public JuristischePersonViewModel(int id, IAppImplementation impl) : this(impl.ctx.JuristischePersonen.Find(id), impl) { }
        public JuristischePersonViewModel(JuristischePerson j, IAppImplementation impl)
        {
            Entity = j;
            Id = j.JuristischePersonId;
            Impl = impl;

            UpdateListen();

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
                Impl.ctx.JuristischePersonen.Update(GetEntity);
            }
            else
            {
                Impl.ctx.JuristischePersonen.Add(GetEntity);
            }
            Impl.SaveWalter();
        }
    }
}
