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
    public sealed class ErhaltungsaufwendungDetailViewModel : DetailViewModel<Erhaltungsaufwendung>, IDetailViewModel
    {
        public override string ToString() => Entity.Bezeichnung;

        public int Id => Entity.ErhaltungsaufwendungId;

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Personen = new();
        public ObservableProperty<string> QuickPerson = new(); // TODO what is a QuickPerson?

        public List<WohnungListViewModelEntry> Wohnungen { get; private set; }

        public SavableProperty<WohnungListViewModelEntry> Wohnung { get; private set; }
        public SavableProperty<KontaktListViewModelEntry> Aussteller { get; private set; }
        public SavableProperty<string> Bezeichnung { get; private set; }
        public SavableProperty<double> Betrag { get; private set; }
        public SavableProperty<DateTimeOffset> Datum { get; private set; }
        public SavableProperty<string> Notiz { get; private set; }

        public ErhaltungsaufwendungDetailViewModel(INotificationService ns, IWalterDbService db) : base(ns, db)
        {
            Save = new RelayCommand(_ =>
            {
                Entity.Betrag = Betrag.Value;
                Entity.Bezeichnung = Bezeichnung.Value;
                Entity.Datum = Datum.Value.UtcDateTime;
                Entity.Notiz = Notiz.Value;
                Entity.AusstellerId = Aussteller.Value.Entity.PersonId;
                Entity.Wohnung = Wohnung.Value.Entity;

                save();
            }, _ => true);
            AttachFile = new AsyncRelayCommand(async _ =>
               /* TODO */await Task.FromResult<object>(null), _ => false);
        }

        public override void SetEntity(Erhaltungsaufwendung e)
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
                .Select(k => new KontaktListViewModelEntry(k))
                .ToList()
                .Concat(WalterDbService.ctx.JuristischePersonen
                    .Select(k => new KontaktListViewModelEntry(k))
                    .ToList())
                    .ToImmutableList();
            Aussteller = new(this, Personen.Value.SingleOrDefault(s => s.Entity.PersonId == e.AusstellerId));
        }

        public AsyncRelayCommand AttachFile;

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Betrag != Betrag.Value ||
                Entity.Bezeichnung != Bezeichnung.Value ||
                Entity.Datum != Datum.Value.UtcDateTime ||
                Entity.Notiz != Notiz.Value ||
                Entity.AusstellerId != Aussteller.Value.Entity.PersonId ||
                Entity.Wohnung != Wohnung.Value.Entity;
        }
    }
}
