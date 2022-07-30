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
    public sealed class VertragDetailViewModel : VertragDetailViewModelVersion, IDetail
    {
        public Guid guid { get; }
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> AlleMieter = new();
        public ObservableProperty<KontaktListViewModelEntry> AddMieter = new();

        public void UpdateMieterList()
        {
            AlleMieter.Value = Db.ctx.JuristischePersonen
                .ToImmutableList()
                .Where(j => j.isMieter == true).Select(j => new KontaktListViewModelEntry(j))
                .Concat(Db.ctx.NatuerlichePersonen
                    .Where(n => n.isMieter == true).Select(n => new KontaktListViewModelEntry(n)))
                 .Where(p => !Mieter.Value.Exists(e => p.Entity.PersonId == e.Entity.PersonId))
                .ToImmutableList();
        }

        public List<WohnungListViewModelEntry> AlleWohnungen = new List<WohnungListViewModelEntry>();
        public List<KontaktListViewModelEntry> AlleKontakte;

        public ObservableProperty<ImmutableList<VertragDetailViewModelVersion>> Versionen = new();
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Mieter = new();
        public DateTimeOffset? AddVersionDatum;

        public DateTimeOffset lastBeginn => Versionen.Value.Last().Beginn.Value;
        public DateTimeOffset? firstEnde => Versionen.Value.First().Ende.Value;
        public int StartJahr => Versionen.Value.Last().Beginn.Value.Year;
        public int EndeJahr => Versionen.Value.First().Ende.Value?.Year ?? 9999;

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

            Mieter.Value = db.ctx.MieterSet
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new KontaktListViewModelEntry(db, m.PersonId))
                .ToImmutableList();

            UpdateMieterList();

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

            AddMieterCommand = new RelayCommand(_ =>
            {
                if (AddMieter.Value?.Entity.PersonId is Guid mieterGuid)
                {
                    Mieter.Value = Mieter.Value.Add(new KontaktListViewModelEntry(Db, mieterGuid));
                    UpdateMieterList();
                    Db.ctx.MieterSet.Add(new Mieter()
                    {
                        VertragId = guid,
                        PersonId = mieterGuid,
                    });
                    Db.SaveWalter();
                }
            }, _ => true);

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
            Versionen.Value.ForEach(v => v.versionSave());
            Db.SaveWalter();
            NotificationService.outOfSync = false;
        }

        public RelayCommand AddMiete { get; }
        public RelayCommand AddMieterCommand { get; }
        public RelayCommand AddMietMinderung { get; }
        public RelayCommand AddVersion { get; }
        public AsyncRelayCommand RemoveVersion { get; }
    }
}
