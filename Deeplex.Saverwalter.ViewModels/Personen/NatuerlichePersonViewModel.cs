using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class NatuerlichePersonViewModel : PersonViewModel
    {
        public NatuerlichePerson GetEntity => (NatuerlichePerson)Entity;

        public int Id { get; }

        public List<Anrede> Anreden { get; }

        public async void selfDestruct()
        {
            if (await Impl.Confirmation())
            {
                Avm.ctx.NatuerlichePersonen.Remove(GetEntity);
                Avm.SaveWalter();
            }
        }

        public Anrede Anrede
        {
            get => Entity.Anrede;
            set
            {
                var old = Entity.Anrede;
                Entity.Anrede = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Vorname
        {
            get => GetEntity.Vorname;
            set
            {
                var old = GetEntity.Vorname;
                GetEntity.Vorname = value;
                RaisePropertyChangedAuto(old, value);
            }
        }
        public string Nachname
        {
            get => GetEntity.Nachname;
            set
            {
                var old = GetEntity.Nachname;
                GetEntity.Nachname = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Name => Vorname + " " + Nachname;

        public bool WohnungenInklusiveJurPers
        {
            get => mInklusiveZusatz;
            set
            {
                mInklusiveZusatz = value;
                UpdateListen();
            }
        }

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> JuristischePersonen
            = new ObservableProperty<ImmutableList<KontaktListViewModelEntry>>();

        public void UpdateListen()
        {
            JuristischePersonen.Value = Avm.ctx.JuristischePersonenMitglieder
                .Include(w => w.JuristischePerson)
                .Where(w => w.PersonId == Entity.PersonId)
                .Select(w => new KontaktListViewModelEntry(w.JuristischePerson.PersonId, Avm))
                .ToImmutableList();

            Wohnungen.Value = Avm.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == GetEntity.PersonId ||
                    (WohnungenInklusiveJurPers && JuristischePersonen.Value.Any(m => m.Entity.PersonId == w.BesitzerId)))
                .Select(w => new WohnungListViewModelEntry(w, Avm))
                .ToImmutableList();
        }

        public NatuerlichePersonViewModel(int id, IAppImplementation impl, IWalterDbService db) : this(db.ctx.NatuerlichePersonen.Find(id), impl, db) { }
        public NatuerlichePersonViewModel(IAppImplementation impl, IWalterDbService db) : this(new NatuerlichePerson(), impl, db) { }
        public NatuerlichePersonViewModel(NatuerlichePerson k, IAppImplementation impl, IWalterDbService db) : base(impl, db)
        {
            Entity = k;
            Id = k.NatuerlichePersonId;

            Anreden = Enums.Anreden;
            UpdateListen();

            PropertyChanged += OnUpdate;
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
                case nameof(isHandwerker):
                case nameof(isMieter):
                case nameof(isVermieter):
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
                Avm.ctx.NatuerlichePersonen.Update(GetEntity);
            }
            else
            {
                Avm.ctx.NatuerlichePersonen.Add(GetEntity);
            }
            Avm.SaveWalter();
        }
    }
}
