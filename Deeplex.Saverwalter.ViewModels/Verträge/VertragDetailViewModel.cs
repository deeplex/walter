using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragDetailViewModel : VertragDetailViewModelVersion, IDetailViewModel
    {
        public Guid guid { get; }

        public List<WohnungListViewModelEntry> AlleWohnungen;
        public List<KontaktListViewModelEntry> AlleKontakte;

        public ObservableProperty<ImmutableList<VertragDetailViewModelVersion>> Versionen = new();
        public DateTimeOffset? AddVersionDatum;

        public DateTimeOffset lastBeginn => Versionen.Value.Last().Beginn.Value;
        public DateTimeOffset? firstEnde => Versionen.Value.First().Ende.Value;
        public int StartJahr => Versionen.Value.Last().Beginn.Value.Year;
        public int EndeJahr => Versionen.Value.First().Ende.Value?.Year ?? 9999;

        public ObservableProperty<MietenListViewModel> Mieten { get; private set; } = new();
        public ObservableProperty<MietMinderungListViewModel> MietMinderungen { get; private set; } = new();
        public ObservableProperty<KontaktListViewModel> Mieter { get; private set; } = new();

        public new KontaktListViewModelEntry Ansprechpartner
        {
            get => Versionen.Value.Last().Ansprechpartner.Value;
            set
            {
                Versionen.Value.Last().Ansprechpartner.Value = value;
                RaisePropertyChangedAuto();
            }
        }

        public VertragDetailViewModel(INotificationService ns, IWalterDbService db) : this(
            new List<Vertrag> { new Vertrag { Beginn = DateTime.UtcNow.Date, } }, ns, db)
        { }

        public VertragDetailViewModel(Guid id, INotificationService ns, IWalterDbService db)
            : this(db.ctx.Vertraege
                  .Where(v => v.VertragId == id)
                  .Include(v => v.Wohnung)
                  .ToList()
                  .OrderBy(v => v.Version)
                  .Reverse()
                  .ToList(), ns, db)
        { }

        public VertragDetailViewModel(List<Vertrag> v, INotificationService ns, IWalterDbService db) : base(v.OrderBy(vs => vs.Version).Last(), ns, db)
        {
            guid = v.First().VertragId;

            Mieten.Value = new(guid, ns, db);
            MietMinderungen.Value = new(guid, ns, db);
            Mieter.Value = new(db, ns, v.First());

            Versionen.Value = v.Select(vs => new VertragDetailViewModelVersion(vs, ns, db)).ToImmutableList();

            AlleWohnungen = db.ctx.Wohnungen.Select(w => new WohnungListViewModelEntry(w, db)).ToList();
            Wohnung.Value = AlleWohnungen.Find(w => w.Id == v.First().WohnungId);

            AlleKontakte = db.ctx.JuristischePersonen.ToList().Select(j => new KontaktListViewModelEntry(j))
                    .Concat(db.ctx.NatuerlichePersonen.Select(n => new KontaktListViewModelEntry(n)))
                    .ToList();
            if (Versionen.Value.Last().Ansprechpartner.Value is KontaktListViewModelEntry partner)
            {
                Ansprechpartner = AlleKontakte.Find(e => e.Entity.PersonId == partner.Guid);
            }

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Versionen.Value.ForEach(v =>
                    {
                        Db.ctx.Vertraege.Remove(v.Entity);
                    });
                    Db.SaveWalter();
                }
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);

            AddVersion = new RelayCommand(_ =>
            {
                var last = Versionen.Value.First().Entity;
                var entity = new Vertrag(last, AddVersionDatum?.UtcDateTime ?? DateTime.UtcNow.Date)
                {
                    Personenzahl = Personenzahl.Value,
                    //KaltMiete = KaltMiete, TODO
                };
                var nv = new VertragDetailViewModelVersion(entity, ns, db);
                Versionen.Value = Versionen.Value.Insert(0, nv);
                db.ctx.Vertraege.Add(entity);
                db.SaveWalter();
            }, _ => true);

            RemoveVersion = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    var vs = Versionen.Value.First().Entity;
                    db.ctx.Vertraege.Remove(vs);
                    Versionen.Value = Versionen.Value.Skip(1).ToImmutableList();
                    db.SaveWalter();
                }
            }, _ => true);
        }

        public new void checkForChanges()
        {
            Versionen.Value.ForEach(v => v.checkForChanges());
        }

        private void save()
        {
            Mieten.Value.Liste.Value.ForEach(e => e.save());
            MietMinderungen.Value.Liste.Value.ForEach(e => e.save());

            Versionen.Value.ForEach(v => v.versionSave());
            Db.SaveWalter();
            NotificationService.outOfSync = false;
        }

        public RelayCommand AddVersion { get; }
        public AsyncRelayCommand RemoveVersion { get; }
    }
}
