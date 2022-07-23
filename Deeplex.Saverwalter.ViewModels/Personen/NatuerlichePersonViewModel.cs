using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class NatuerlichePersonViewModel : PersonViewModel, IDetail
    {
        public new NatuerlichePerson Entity => (NatuerlichePerson)base.Entity;

        public int Id { get; }

        public List<Anrede> Anreden { get; }

        public SavableProperty<Anrede> Anrede { get; }
        public SavableProperty<string> Vorname { get; }
        public SavableProperty<string> Nachname { get; }

        public override string ToString() => Vorname + " " + Nachname;

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
        public NatuerlichePersonViewModel(NatuerlichePerson k, INotificationService ns, IWalterDbService db) : base(k, ns, db)
        {
            base.Entity = k;
            Id = k.NatuerlichePersonId;
            Anrede = new(this, k.Anrede);
            Vorname = new(this, k.Vorname);
            Nachname = new(this, k.Nachname);

            Anreden = Enums.Anreden;
            UpdateListen();

            Save = new RelayCommand(_ =>
            {
                save();
                Entity.Nachname = Nachname.Value;
                Entity.Vorname = Vorname.Value;
                Entity.Anrede = Anrede.Value;

                if (Entity.NatuerlichePersonId != 0)
                {
                    Db.ctx.NatuerlichePersonen.Update(Entity);
                }
                else
                {
                    Db.ctx.NatuerlichePersonen.Add(Entity);
                }
                Db.SaveWalter();
            }, _ => true);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Db.ctx.NatuerlichePersonen.Remove(Entity);
                    Db.SaveWalter();
                }
            }, _ => true);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                BaseCheckForChanges() ||
                    Entity.Anrede != Anrede.Value ||
                    Entity.Vorname != Vorname.Value ||
                    Entity.Nachname != Nachname.Value;
        }
    }
}
