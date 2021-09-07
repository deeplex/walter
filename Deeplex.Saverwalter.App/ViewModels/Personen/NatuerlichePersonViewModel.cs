using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Deeplex.Saverwalter.ViewModels.Utils;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class NatuerlichePersonViewModel : PersonViewModel
    {
        public NatuerlichePerson GetEntity => (NatuerlichePerson)Entity;

        public int Id { get; }

        public List<Anrede> Anreden { get; }

        public async void selfDestruct()
        {
            Impl.ctx.NatuerlichePersonen.Remove(GetEntity);
            Impl.SaveWalter();
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

        public ObservableProperty<ImmutableList<KontaktListEntry>> JuristischePersonen
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();

        public void UpdateListen()
        {
            JuristischePersonen.Value = Impl.ctx.JuristischePersonenMitglieder
                .Include(w => w.JuristischePerson)
                .Where(w => w.PersonId == Entity.PersonId)
                .Select(w => new KontaktListEntry(w.JuristischePerson.PersonId, Impl))
                .ToImmutableList();

            Wohnungen.Value = Impl.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == GetEntity.PersonId ||
                    (WohnungenInklusiveJurPers && JuristischePersonen.Value.Any(m => m.Guid == w.BesitzerId)))
                .Select(w => new WohnungListEntry(w, Impl))
                .ToImmutableList();
        }

        private IAppImplementation Impl;

        public NatuerlichePersonViewModel(int id, IAppImplementation impl) : this(impl.ctx.NatuerlichePersonen.Find(id), impl) { }
        public NatuerlichePersonViewModel(IAppImplementation impl) : this(new NatuerlichePerson(), impl) { }
        public NatuerlichePersonViewModel(NatuerlichePerson k, IAppImplementation impl) : base(impl)
        {
            Impl = impl;

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
                Impl.ctx.NatuerlichePersonen.Update(GetEntity);
            }
            else
            {
                Impl.ctx.NatuerlichePersonen.Add(GetEntity);
            }
            Impl.SaveWalter();
        }
    }
}
