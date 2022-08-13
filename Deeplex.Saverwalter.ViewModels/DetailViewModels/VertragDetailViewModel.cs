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
        public Guid guid { get; private set; }

        public List<WohnungListViewModelEntry> AlleWohnungen;
        public List<KontaktListViewModelEntry> AlleKontakte;

        public ObservableProperty<ImmutableList<VertragDetailViewModelVersion>> Versionen = new();
        public DateTimeOffset? AddVersionDatum;

        public DateTimeOffset lastBeginn => Versionen.Value.Last().Beginn.Value;
        public DateTimeOffset? firstEnde => Versionen.Value.First().Ende.Value;
        public int StartJahr => Versionen.Value.Last().Beginn.Value.Year;
        public int EndeJahr => Versionen.Value.First().Ende.Value?.Year ?? 9999;

        public ObservableProperty<MieteListViewModel> Mieten { get; private set; } = new();
        public ObservableProperty<MietminderungListViewModel> MietMinderungen { get; private set; } = new();
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

        public new WohnungListViewModelEntry Wohnung
        {
            get => Versionen.Value.Last().Wohnung.Value;
            set
            {
                Versionen.Value.Last().Wohnung.Value = value;
                RaisePropertyChangedAuto();
            }
        }

        public new void SetEntity(Vertrag v)
        {
            var list = WalterDbService.ctx.Vertraege
                  .Where(e => e.VertragId == v.VertragId)
                  .Include(e => e.Wohnung)
                  .ToList()
                  .OrderBy(e => e.Version)
                  .Reverse()
                  .ToList();
            guid = list.First().VertragId;

            Mieten.Value = new(guid, NotificationService, WalterDbService);
            MietMinderungen.Value = new(guid, NotificationService, WalterDbService);
            Mieter.Value = new(WalterDbService, NotificationService);
            Mieter.Value.SetList(list.First());

            Versionen.Value = list.Select(vs =>
            {
                var r = new VertragDetailViewModelVersion(NotificationService, WalterDbService);
                r.SetEntity(vs);
                return r;
            }).ToImmutableList();

            Wohnung = AlleWohnungen.Find(w => w.Id == list.First().WohnungId);

            if (Versionen.Value.Last().Ansprechpartner.Value is KontaktListViewModelEntry partner)
            {
                Ansprechpartner = AlleKontakte.Find(e => e.Entity.PersonId == partner.Guid);
            }
        }

        public VertragDetailViewModel(INotificationService ns, IWalterDbService db) : base(ns, db)
        {
            AlleWohnungen = db.ctx.Wohnungen.Select(w => new WohnungListViewModelEntry(w, db)).ToList();

            AlleKontakte = db.ctx.JuristischePersonen.ToList().Select(j => new KontaktListViewModelEntry(j))
                    .Concat(db.ctx.NatuerlichePersonen.Select(n => new KontaktListViewModelEntry(n)))
                    .ToList();

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Versionen.Value.ForEach(v =>
                    {
                        WalterDbService.ctx.Vertraege.Remove(v.Entity);
                    });
                    WalterDbService.SaveWalter();
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
                var nv = new VertragDetailViewModelVersion(ns, db);
                nv.SetEntity(entity);

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
            WalterDbService.SaveWalter();
            RaisePropertyChanged(nameof(isInitialized));

            checkForChanges();
        }

        public RelayCommand AddVersion { get; }
        public AsyncRelayCommand RemoveVersion { get; }
    }
}
