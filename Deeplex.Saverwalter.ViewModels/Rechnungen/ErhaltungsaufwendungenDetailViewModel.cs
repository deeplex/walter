using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenDetailViewModel : BindableBase, IDetail
    {
        public override string ToString() => Entity.Bezeichnung;

        public Erhaltungsaufwendung Entity { get; }
        public int Id => Entity.ErhaltungsaufwendungId;

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Personen = new();
        public ObservableProperty<string> QuickPerson = new();

        public List<WohnungListViewModelEntry> Wohnungen { get; }

        public SavableProperty<WohnungListViewModelEntry> Wohnung { get; }
        public SavableProperty<KontaktListViewModelEntry> Aussteller { get; }
        public SavableProperty<string> Bezeichnung { get; }
        public SavableProperty<double> Betrag { get; }
        public SavableProperty<DateTimeOffset> Datum { get; }
        public SavableProperty<string> Notiz { get; }

        private IWalterDbService Db;
        private INotificationService NotificationService;

        public AsyncRelayCommand Delete { get; }
        public RelayCommand Save { get; }

        public ErhaltungsaufwendungenDetailViewModel(INotificationService ns, IWalterDbService db) : this(new Erhaltungsaufwendung(), ns, db) { }
        public ErhaltungsaufwendungenDetailViewModel(Erhaltungsaufwendung e, INotificationService ns, IWalterDbService db)
        {
            Entity = e;
            Db = db;
            NotificationService = ns;

            Bezeichnung = new(this, e.Bezeichnung);
            Betrag = new(this, e.Betrag);
            Datum = new(this, e.Datum.AsUtcKind());
            Notiz = new(this, e.Notiz);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Db.ctx.Erhaltungsaufwendungen.Remove(Entity);
                    Db.SaveWalter();
                }
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);

            Wohnungen = Db.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListViewModelEntry(w, db)).ToList();
            Wohnung = new(this, Wohnungen.Find(f => f.Id == e.Wohnung?.WohnungId));

            Personen.Value = Db.ctx.NatuerlichePersonen
                .Where(w => w.isHandwerker)
                .Select(k => new KontaktListViewModelEntry(k))
                .ToList()
                .Concat(Db.ctx.JuristischePersonen
                    .Where(w => w.isHandwerker)
                    .Select(k => new KontaktListViewModelEntry(k))
                    .ToList())
                    .ToImmutableList();
            Aussteller = new(this, Personen.Value.SingleOrDefault(s => s.Entity.PersonId == e.AusstellerId));

            AttachFile = new AsyncRelayCommand(async _ =>
               /* TODO */await Task.FromResult<object>(null), _ => false);
        }

        public AsyncRelayCommand AttachFile;

        public void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Betrag != Betrag.Value ||
                Entity.Bezeichnung != Bezeichnung.Value ||
                Entity.Datum != Datum.Value.UtcDateTime ||
                Entity.Notiz != Notiz.Value ||
                Entity.AusstellerId != Aussteller.Value.Entity.PersonId ||
                Entity.Wohnung != Wohnung.Value.Entity;
        }

        private void save()
        {
            Entity.Betrag = Betrag.Value;
            Entity.Bezeichnung = Bezeichnung.Value;
            Entity.Datum = Datum.Value.UtcDateTime;
            Entity.Notiz = Notiz.Value;
            Entity.AusstellerId = Aussteller.Value.Entity.PersonId;
            Entity.Wohnung = Wohnung.Value.Entity;

            if (Entity.ErhaltungsaufwendungId != 0)
            {
                Db.ctx.Erhaltungsaufwendungen.Update(Entity);
            }
            else
            {
                Db.ctx.Erhaltungsaufwendungen.Add(Entity);
            }
            Db.SaveWalter();
            NotificationService.outOfSync = false;
        }
    }
}
