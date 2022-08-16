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
        public Guid guid => Entity.VertragId;

        public List<WohnungListViewModelEntry> AlleWohnungen;
        public List<KontaktListViewModelEntry> AlleKontakte;

        public ObservableProperty<ImmutableList<VertragDetailViewModelVersion>> Versionen = new();
        public DateTimeOffset? AddVersionDatum;

        public DateTimeOffset lastBeginn => Versionen.Value.Last().Beginn.Value;
        public DateTimeOffset? firstEnde => Versionen.Value.First().Ende.Value;
        public int StartJahr => Versionen.Value.Last().Beginn.Value.Year;
        public int EndeJahr => Versionen.Value.First().Ende.Value?.Year ?? 9999;

        public ObservableProperty<MieteListViewModel> Mieten { get; private set; } = new();
        public ObservableProperty<MietminderungListViewModel> Mietminderungen { get; private set; } = new();
        public ObservableProperty<KontaktListViewModel> Mieter { get; private set; } = new();
        public MemberViewModel<Vertrag> SelectMieter { get; private set; }

        public new KontaktListViewModelEntry Ansprechpartner
        {
            get => Versionen.Value.LastOrDefault()?.Ansprechpartner.Value;
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
                if (Versionen.Value.LastOrDefault() is VertragDetailViewModelVersion v)
                {
                    v.Wohnung.Value = value;
                }
                RaisePropertyChanged(nameof(Vermieter));
                RaisePropertyChangedAuto();
            }
        }

        public string Vermieter => Versionen.Value.Last().Wohnung.Value.Besitzer;

        public new void SetEntity(Vertrag v)
        {
            Entity = v;
            var list = WalterDbService.ctx.Vertraege
                  .Where(e => e.VertragId == v.VertragId)
                  .Include(e => e.Wohnung)
                  .ToList()
                  .OrderBy(e => e.Version)
                  .Reverse()
                  .ToList();

            Mieten.Value = new(guid, NotificationService, WalterDbService);
            Mietminderungen.Value = new(guid, NotificationService, WalterDbService);
            Mieter.Value = new(WalterDbService, NotificationService);
            Mieter.Value.SetList(list.FirstOrDefault());

            Versionen.Value = list.Select(vs =>
            {
                var r = new VertragDetailViewModelVersion(NotificationService, WalterDbService);
                r.SetEntity(vs);
                return r;
            }).ToImmutableList();

            Wohnung = AlleWohnungen.Find(w => w.Id == list.FirstOrDefault()?.WohnungId);

            if (Versionen.Value.LastOrDefault()?.Ansprechpartner.Value is KontaktListViewModelEntry partner)
            {
                Ansprechpartner = AlleKontakte.Find(e => e.Entity.PersonId == partner.Guid);
            }

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
                    Versionen.Value.ForEach(v =>
                    {
                        WalterDbService.ctx.Vertraege.Remove(v.Entity);
                    });
                    WalterDbService.SaveWalter();
                }
            }, _ => true);

            Save = new RelayCommand(_ =>
            {
                Mieten.Value.Liste.Value.ForEach(e => e.save());
                Mietminderungen.Value.Liste.Value.ForEach(e => e.save());

                Versionen.Value.ForEach(v => v.versionSave());
            }, _ => true);

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

        public RelayCommand AddVersion { get; }
        public AsyncRelayCommand RemoveVersion { get; }
    }
}
