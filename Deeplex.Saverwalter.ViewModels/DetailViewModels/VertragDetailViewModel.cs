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
    public sealed class VertragDetailViewModel : DetailViewModel<Vertrag>, IDetailViewModel
    {
        public override string ToString() => "Vertrag";

        public int Id => Entity.VertragId;

        public List<WohnungListViewModelEntry> AlleWohnungen;
        public List<KontaktListViewModelEntry> AlleKontakte;

        public ObservableProperty<ImmutableList<VertragDetailViewModelVersion>> Versionen = new();
        public DateTimeOffset? AddVersionDatum;

        public DateTimeOffset Beginn => Entity.Beginn().AsUtcKind();
        public DateTimeOffset? Ende => Entity.Ende()?.AsUtcKind();
        public int StartJahr => Beginn.Year;
        public int EndeJahr => Ende?.Year ?? 9999;

        public ObservableProperty<MieteListViewModel> Mieten { get; private set; } = new();
        public ObservableProperty<MietminderungListViewModel> Mietminderungen { get; private set; } = new();
        public ObservableProperty<KontaktListViewModel> Mieter { get; private set; } = new();
        public MemberViewModel<Vertrag> SelectMieter { get; private set; }

        public SavableProperty<KontaktListViewModelEntry> Ansprechpartner { get; set; }
        public SavableProperty<string> Notiz { get; private set; }

        private WohnungListViewModelEntry mWohnung { get; set; }
        public WohnungListViewModelEntry Wohnung
        {
            get => AlleWohnungen.Find(e => e.Id == mWohnung.Id);
            set
            {
                mWohnung = value;
                checkForChanges();
                RaisePropertyChanged(nameof(Vermieter));
                RaisePropertyChangedAuto();
            }
        }

        public string Vermieter => Versionen.Value.Last().Wohnung.Value.Besitzer;

        public override void SetEntity(Vertrag v)
        {
            Entity = v;

            Mieten.Value = new(v, NotificationService, WalterDbService);
            Mietminderungen.Value = new(v, NotificationService, WalterDbService);
            Mieter.Value = new(WalterDbService, NotificationService);
            Mieter.Value.SetList(v);

            Versionen.Value = v.Versionen.Select(vs =>
            {
                var r = new VertragDetailViewModelVersion(NotificationService, WalterDbService);
                r.SetEntity(vs);
                return r;
            }).ToImmutableList();

            Wohnung = AlleWohnungen.Find(w => w.Id == v.Wohnung.WohnungId);
            Ansprechpartner = new(this, AlleKontakte.Find(e => e.Entity.PersonId == v.AnsprechpartnerId));
            Notiz = new(this, v.Notiz);

            SelectMieter = new(WalterDbService, NotificationService);
            SelectMieter.SetList(v, Mieter.Value);
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
                    WalterDbService.ctx.Vertraege.Remove(Entity);
                    WalterDbService.SaveWalter();
                }
            }, _ => true);

            Save = new RelayCommand(_ =>
            {
                Mieten.Value.Liste.Value.ForEach(e => e.save());
                Mietminderungen.Value.Liste.Value.ForEach(e => e.save());
                Versionen.Value.ForEach(v => v.versionSave());

                Entity.Notiz = Notiz.Value;
                Entity.Wohnung = Wohnung.Entity;
                Entity.AnsprechpartnerId = Ansprechpartner.Value.Guid;

                save();
            }, _ => true);

            AddVersion = new RelayCommand(_ =>
            {
                var last = Entity.Versionen.OrderBy(e => e.Beginn).Last();
                var entity = new VertragVersion()
                {
                    Personenzahl = last.Personenzahl,
                    Grundmiete = last.Grundmiete,
                    Vertrag = Entity,
                };
                var nv = new VertragDetailViewModelVersion(ns, db);
                nv.SetEntity(entity);

                Versionen.Value = Versionen.Value.Prepend(nv).ToImmutableList();
                db.ctx.VertragVersionen.Add(entity);
                db.SaveWalter();
            }, _ => true);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Wohnung.Id != Entity.Wohnung.WohnungId ||
                Ansprechpartner.Value.Guid != Entity.AnsprechpartnerId ||
                Notiz.Value != Entity.Notiz;

            Versionen.Value.ForEach(v => v.checkForChanges());
        }

        public RelayCommand AddVersion { get; }
        public AsyncRelayCommand RemoveVersion { get; }
        public RelayCommand RemoveDate { get; }
    }
}
