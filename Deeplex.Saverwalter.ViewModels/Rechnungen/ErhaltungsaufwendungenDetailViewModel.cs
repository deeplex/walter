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
    public sealed class ErhaltungsaufwendungenDetailViewModel : BindableBase, IDetailViewModel<Erhaltungsaufwendung>
    {
        public override string ToString() => Entity.Bezeichnung;

        public Erhaltungsaufwendung Entity { get; private set; }
        public int Id => Entity.ErhaltungsaufwendungId;

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Personen = new();
        public ObservableProperty<string> QuickPerson = new(); // TODO what is a QuickPerson?

        public List<WohnungListViewModelEntry> Wohnungen { get; private set; }

        public SavableProperty<WohnungListViewModelEntry, Erhaltungsaufwendung> Wohnung { get; private set; }
        public SavableProperty<KontaktListViewModelEntry, Erhaltungsaufwendung> Aussteller { get; private set; }
        public SavableProperty<string, Erhaltungsaufwendung> Bezeichnung { get; private set; }
        public SavableProperty<double, Erhaltungsaufwendung> Betrag { get; private set; }
        public SavableProperty<DateTimeOffset, Erhaltungsaufwendung> Datum { get; private set; }
        public SavableProperty<string, Erhaltungsaufwendung> Notiz { get; private set; }

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }

        public AsyncRelayCommand Delete { get; }
        public RelayCommand Save { get; }

        public ErhaltungsaufwendungenDetailViewModel(INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            NotificationService = ns;

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    WalterDbService.ctx.Erhaltungsaufwendungen.Remove(Entity);
                    WalterDbService.SaveWalter();
                }
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);
            AttachFile = new AsyncRelayCommand(async _ =>
               /* TODO */await Task.FromResult<object>(null), _ => false);
        }

        public void SetEntity(Erhaltungsaufwendung e)
        {
            Entity = e;

            Bezeichnung = new(this, e.Bezeichnung);
            Betrag = new(this, e.Betrag);
            Datum = new(this, e.Datum.AsUtcKind());
            Notiz = new(this, e.Notiz);

            Wohnungen = WalterDbService.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListViewModelEntry(w, WalterDbService)).ToList();
            Wohnung = new(this, Wohnungen.Find(f => f.Id == e.Wohnung?.WohnungId));

            Personen.Value = WalterDbService.ctx.NatuerlichePersonen
                .Where(w => w.isHandwerker)
                .Select(k => new KontaktListViewModelEntry(k))
                .ToList()
                .Concat(WalterDbService.ctx.JuristischePersonen
                    .Where(w => w.isHandwerker)
                    .Select(k => new KontaktListViewModelEntry(k))
                    .ToList())
                    .ToImmutableList();
            Aussteller = new(this, Personen.Value.SingleOrDefault(s => s.Entity.PersonId == e.AusstellerId));
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
                WalterDbService.ctx.Erhaltungsaufwendungen.Update(Entity);
            }
            else
            {
                WalterDbService.ctx.Erhaltungsaufwendungen.Add(Entity);
            }
            WalterDbService.SaveWalter();
            NotificationService.outOfSync = false;
        }
    }
}
