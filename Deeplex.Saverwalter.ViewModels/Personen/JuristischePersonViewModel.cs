using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class JuristischePersonViewModel : PersonViewModel, IDetail
    {
        public new JuristischePerson Entity => (JuristischePerson)base.Entity;
        public int Id;

        public SavableProperty<string> Bezeichnung { get; }
        public override string ToString() => Bezeichnung.Value;

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Mitglieder = new();
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> AddMitglieder = new();
        public ObservableProperty<KontaktListViewModelEntry> AddMitglied = new();

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
            Mitglieder.Value = Entity.Mitglieder
                .Select(w => new KontaktListViewModelEntry(Db, w.PersonId))
                .ToImmutableList();

            AddMitglieder.Value = Db.ctx.NatuerlichePersonen
                .Select(k => new KontaktListViewModelEntry(k))
                .ToList()
                .Concat(Db.ctx.JuristischePersonen
                    .Select(k => new KontaktListViewModelEntry(k))
                    .ToList())
                .Where(k => !Mitglieder.Value.Any(e => e.Entity.PersonId == k.Entity.PersonId))
                    .ToImmutableList();

            Wohnungen.Value = Db.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == Entity.PersonId ||
                    (WohnungenInklusiveMitglieder && Mitglieder.Value.Any(m => m.Entity.PersonId == w.BesitzerId)))
                .Select(w => new WohnungListViewModelEntry(w, Db))
                .ToImmutableList();
        }

        public RelayCommand AddMitgliedCommand;

        public JuristischePersonViewModel(INotificationService ns, IWalterDbService db) : this(new JuristischePerson(), ns, db) { }
        public JuristischePersonViewModel(int id, INotificationService ns, IWalterDbService db) : this(db.ctx.JuristischePersonen.Find(id), ns, db) { }
        public JuristischePersonViewModel(JuristischePerson j, INotificationService ns, IWalterDbService db) : base(j, ns, db)
        {
            base.Entity = j;
            Id = j.JuristischePersonId;
            Bezeichnung = new(this, j.Bezeichnung);

            UpdateListen();

            AddMitgliedCommand = new RelayCommand(_ =>
            {
                if (AddMitglied.Value?.Entity.PersonId is Guid guid)
                {
                    if (AddMitglied.Value.Entity is NatuerlichePerson n)
                    {
                        n.JuristischePersonen.Add(Entity);
                    }
                    else if (AddMitglied.Value.Entity is JuristischePerson j)
                    {
                        j.JuristischePersonen.Add(Entity);
                    }
                    Db.SaveWalter();
                    UpdateListen();
                }
            }, _ => true);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Db.ctx.JuristischePersonen.Remove(Entity);
                    Db.SaveWalter();
                }
            }, _ => true);

            Save = new RelayCommand(_ =>
            {
                save();
                Entity.Bezeichnung = Bezeichnung.Value;

                if (Entity.JuristischePersonId != 0)
                {
                    Db.ctx.JuristischePersonen.Update(Entity);
                }
                else
                {
                    Db.ctx.JuristischePersonen.Add(Entity);
                }
                Db.SaveWalter();
            }, _ => true);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                BaseCheckForChanges() ||
                Entity.Bezeichnung != Bezeichnung.Value;
        }
    }
}
