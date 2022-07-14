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
        public new NatuerlichePerson Entity => (NatuerlichePerson)base.Entity;

        public int Id { get; }

        public List<Anrede> Anreden { get; }

        public async void selfDestruct()
        {
            if (await NotificationService.Confirmation())
            {
                Db.ctx.NatuerlichePersonen.Remove(Entity);
                Db.SaveWalter();
            }
        }

        public Anrede Anrede
        {
            get => base.Entity.Anrede;
            set
            {
                var old = base.Entity.Anrede;
                base.Entity.Anrede = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Vorname
        {
            get => Entity.Vorname;
            set
            {
                var old = Entity.Vorname;
                Entity.Vorname = value;
                RaisePropertyChangedAuto(old, value);
            }
        }
        public string Nachname
        {
            get => Entity.Nachname;
            set
            {
                var old = Entity.Nachname;
                Entity.Nachname = value;
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

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> JuristischePersonen = new();

        public void UpdateListen()
        {
            JuristischePersonen.Value = Entity.JuristischePersonen
                .Select(w => new KontaktListViewModelEntry(w.PersonId, Db))
                .ToImmutableList();

            Wohnungen.Value = Db.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == Entity.PersonId ||
                    (WohnungenInklusiveJurPers && JuristischePersonen.Value.Any(m => m.Entity.PersonId == w.BesitzerId)))
                .Select(w => new WohnungListViewModelEntry(w, Db))
                .ToImmutableList();
        }

        public NatuerlichePersonViewModel(int id, INotificationService ns, IWalterDbService db) : this(db.ctx.NatuerlichePersonen.Find(id), ns, db) { }
        public NatuerlichePersonViewModel(INotificationService ns, IWalterDbService db) : this(new NatuerlichePerson(), ns, db) { }
        public NatuerlichePersonViewModel(NatuerlichePerson k, INotificationService ns, IWalterDbService db) : base(ns, db)
        {
            base.Entity = k;
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

            if (Entity.Nachname == null)
            {
                return;
            }

            if (Entity.NatuerlichePersonId != 0)
            {
                Db.ctx.NatuerlichePersonen.Update(Entity);
            }
            else
            {
                Db.ctx.NatuerlichePersonen.Add(Entity);
            }
            Db.SaveWalter();
        }
    }
}
