using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class JuristischePersonViewModel : PersonViewModel
    {
        public new JuristischePerson Entity => (JuristischePerson)base.Entity;
        public int Id;

        public string Bezeichnung
        {
            get => base.Entity.Bezeichnung;
            set
            {
                var old = Entity.Bezeichnung;
                Entity.Bezeichnung = value;
                RaisePropertyChangedAuto(old, value);
            }
        }
        public override string ToString() => Bezeichnung;

        public async void selfDestruct()
        {
            if (await Impl.Confirmation())
            {
                Avm.ctx.JuristischePersonen.Remove(Entity);
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
                .Where(k => !Mitglieder.Value.Any(e => e.Entity.PersonId == k.Entity.PersonId))
                    .ToImmutableList();

            Wohnungen.Value = Avm.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == Entity.PersonId ||
                    (WohnungenInklusiveMitglieder && Mitglieder.Value.Any(m => m.Entity.PersonId == w.BesitzerId)))
                .Select(w => new WohnungListViewModelEntry(w, Avm))
                .ToImmutableList();
        }

        public RelayCommand AddMitgliedCommand;

        public JuristischePersonViewModel(IAppImplementation impl, IWalterDbService avm) : this(new JuristischePerson(), impl, avm) { }
        public JuristischePersonViewModel(int id, IAppImplementation impl, IWalterDbService avm) : this(avm.ctx.JuristischePersonen.Find(id), impl, avm) { }
        public JuristischePersonViewModel(JuristischePerson j, IAppImplementation impl, IWalterDbService avm) : base(impl, avm)
        {
            base.Entity = j;
            Id = j.JuristischePersonId;

            UpdateListen();

            PropertyChanged += OnUpdate;
            AddMitgliedCommand = new RelayCommand(_ =>
            {
                if (AddMitglied.Value?.Entity.PersonId is Guid guid)
                {
                    Avm.ctx.JuristischePersonenMitglieder.Add(new JuristischePersonenMitglied()
                    {
                        JuristischePersonId = Id,
                        PersonId = AddMitglied.Value.Entity.PersonId,
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

            if (base.Entity.Bezeichnung == null)
            {
                return;
            }

            if (Entity.JuristischePersonId != 0)
            {
                Avm.ctx.JuristischePersonen.Update(Entity);
            }
            else
            {
                Avm.ctx.JuristischePersonen.Add(Entity);
            }
            Avm.SaveWalter();
        }
    }
}
