﻿using Deeplex.Saverwalter.Model;
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
    public sealed class VertragDetailViewModel : VertragDetailViewModelVersion
    {
        public Guid guid { get; }
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> AlleMieter
            = new ObservableProperty<ImmutableList<KontaktListViewModelEntry>>();
        public ObservableProperty<KontaktListViewModelEntry> AddMieter = new ObservableProperty<KontaktListViewModelEntry>();

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

        public ObservableProperty<ImmutableList<VertragDetailViewModelVersion>> Versionen
            = new ObservableProperty<ImmutableList<VertragDetailViewModelVersion>>();

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Mieter
            = new ObservableProperty<ImmutableList<KontaktListViewModelEntry>>();
        public DateTimeOffset? AddVersionDatum;

        public DateTimeOffset lastBeginn => Versionen.Value.Last().Beginn;
        public DateTimeOffset? firstEnde => Versionen.Value.First().Ende;
        public int StartJahr => Versionen.Value.Last().Beginn.Year;
        public int EndeJahr => Versionen.Value.First().Ende?.Year ?? 9999;

        public VertragDetailViewModel(IAppImplementation impl, IWalterDbService db) : this(new List<Vertrag>
            { new Vertrag { Beginn = DateTime.UtcNow.Date, } }, impl, db)
        { }

        public VertragDetailViewModel(Guid id, IAppImplementation impl, IWalterDbService db)
            : this(db.ctx.Vertraege
                  .Where(v => v.VertragId == id)
                  .Include(v => v.Wohnung)
                  .ToList()
                  .OrderBy(v => v.Version)
                  .Reverse()
                  .ToList(), impl, db)
        { }

        public VertragDetailViewModel(List<Vertrag> v, IAppImplementation impl, IWalterDbService db) : base(v.OrderBy(vs => vs.Version).Last(), impl, db)
        {
            guid = v.First().VertragId;

            Versionen.Value = v.Select(vs => new VertragDetailViewModelVersion(vs, impl, db)).ToImmutableList();

            AlleWohnungen = db.ctx.Wohnungen.Select(w => new WohnungListViewModelEntry(w, db)).ToList();
            Wohnung = AlleWohnungen.Find(w => w.Id == v.First().WohnungId);

            AlleKontakte = db.ctx.JuristischePersonen.ToList().Select(j => new KontaktListViewModelEntry(j))
                    .Concat(db.ctx.NatuerlichePersonen.Select(n => new KontaktListViewModelEntry(n)))
                    .ToList();
            Ansprechpartner = AlleKontakte.Find(w => w.Entity.PersonId == v.First().AnsprechpartnerId);

            Mieter.Value = db.ctx.MieterSet
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new KontaktListViewModelEntry(m.PersonId, db))
                .ToImmutableList();

            UpdateMieterList();

            AddMieterCommand = new RelayCommand(_ =>
            {
                if (AddMieter.Value?.Entity.PersonId is Guid mieterGuid)
                {
                    Mieter.Value = Mieter.Value.Add(new KontaktListViewModelEntry(mieterGuid, Db));
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
                    Personenzahl = Personenzahl,
                    //KaltMiete = KaltMiete, TODO
                };
                var nv = new VertragDetailViewModelVersion(entity, impl, db);
                Versionen.Value = Versionen.Value.Insert(0, nv);
                db.ctx.Vertraege.Add(entity);
                db.SaveWalter();
            }, _ => true);

            RemoveVersion = new AsyncRelayCommand(async _ =>
            {
                if (await impl.Confirmation())
                {
                    var vs = Versionen.Value.First().Entity;
                    db.ctx.Vertraege.Remove(vs);
                    Versionen.Value = Versionen.Value.Skip(1).ToImmutableList();
                    db.SaveWalter();
                }
            }, _ => true);
        }

        public RelayCommand AddMiete { get; }
        public RelayCommand AddMieterCommand { get; }
        public RelayCommand AddMietMinderung { get; }
        public RelayCommand AddVersion { get; }
        public AsyncRelayCommand RemoveVersion { get; }

        public async Task SelfDestruct()
        {
            if (await Impl.Confirmation())
            {
                Versionen.Value.ForEach(v =>
                {
                    Db.ctx.Mieten
                        .Where(m => m.VertragId == guid)
                        .ToList()
                        .ForEach(m => Db.ctx.Mieten.Remove(m));
                    Db.ctx.MietMinderungen
                        .Where(m => m.VertragId == guid)
                        .ToList()
                        .ForEach(m => Db.ctx.MietMinderungen.Remove(m));

                    Db.ctx.Vertraege.Remove(v.Entity);
                });
                Db.SaveWalter();
            }

        }
    }
}
