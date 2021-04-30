﻿using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class VertragDetailViewModel : VertragDetailVersion
    {
        public Guid guid { get; }
        public ObservableProperty<ImmutableList<VertragDetailKontakt>> AlleMieter
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public ImmutableList<VertragDetailWohnung> AlleWohnungen
            => App.Walter.Wohnungen.Select(w => new VertragDetailWohnung(w)).ToImmutableList();

        public ImmutableList<VertragDetailKontakt> AlleKontakte =>
                App.Walter.JuristischePersonen.ToImmutableList().Select(j => new VertragDetailKontakt(j))
                    .Concat(App.Walter.NatuerlichePersonen.Select(n => new VertragDetailKontakt(n)))
                    .ToImmutableList();

        public ObservableProperty<ImmutableList<VertragDetailVersion>> Versionen
            = new ObservableProperty<ImmutableList<VertragDetailVersion>>();
        public ObservableProperty<ImmutableList<VertragDetailKontakt>> Mieter
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public DateTimeOffset? AddVersionDatum;
        public ObservableProperty<int> BetriebskostenJahr
            = new ObservableProperty<int>();

        // TODO Define setter
        public DateTimeOffset lastBeginn
        {
            get => Versionen.Value.Last().Beginn;
        }
        public DateTimeOffset? firstEnde
        {
            get => Versionen.Value.First().Ende;
        }

        public VertragDetailViewModel() : this(new List<Vertrag>
            { new Vertrag { Beginn = DateTime.UtcNow.Date, } })
        { }

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Where(v => v.VertragId == id)
                  .Include(v => v.Wohnung)
                  .ToList()
                  .OrderBy(v => v.Version)
                  .Reverse()
                  .ToList())
        { }

        public VertragDetailViewModel(List<Vertrag> v) : base(v.OrderBy(vs => vs.Version).Last())
        {
            guid = v.First().VertragId;

            Mieter.Value = App.Walter.MieterSet
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new VertragDetailKontakt(m.PersonId))
                .ToImmutableList();

            AlleMieter.Value = App.Walter.JuristischePersonen.ToImmutableList()
                    .Where(j => j.isMieter == true).Select(j => new VertragDetailKontakt(j))
                    .Concat(App.Walter.NatuerlichePersonen
                        .Where(n => n.isMieter == true).Select(n => new VertragDetailKontakt(n)))
                    .Where(p => !Mieter.Value.Exists(e => p.PersonId == e.PersonId))
                    .ToImmutableList();

            BetriebskostenJahr.Value = DateTime.Now.Year - 1;

            Versionen.Value = v.Select(vs => new VertragDetailVersion(vs)).ToImmutableList();

            AddVersion = new RelayCommand(_ =>
            {
                var last = App.Walter.Vertraege.Find(Versionen.Value.First().Id);
                var entity = new Vertrag(last, AddVersionDatum?.UtcDateTime ?? DateTime.UtcNow.Date)
                {
                    Personenzahl = Personenzahl,
                    //KaltMiete = KaltMiete, TODO
                };
                var nv = new VertragDetailVersion(entity);
                Versionen.Value = Versionen.Value.Insert(0, nv);
                App.Walter.Add(entity);
                App.SaveWalter();
            }, _ => true);

            RemoveVersion = new RelayCommand(_ =>
            {
                var vs = App.Walter.Vertraege.Find(Versionen.Value.First().Id);
                App.Walter.Vertraege.Remove(vs);
                Versionen.Value = Versionen.Value.Skip(1).ToImmutableList();
                App.SaveWalter();
            }, _ => true);


            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.VertragAnhaenge, guid), _ => true);
        }

        public AsyncRelayCommand AttachFile { get; }
        public RelayCommand AddMiete { get; }
        public RelayCommand AddMietMinderung { get; }
        public RelayCommand AddVersion { get; }
        public RelayCommand RemoveVersion { get; }
    }

    public sealed class VertragDetailKontakt
    {
        public override string ToString() => Bezeichnung;

        public Guid PersonId;
        public bool isMieter;
        public string Bezeichnung;

        public VertragDetailKontakt(Guid i) : this(App.Walter.FindPerson(i)) { }

        public VertragDetailKontakt(IPerson p)
        {
            PersonId = p.PersonId;
            isMieter = p.isMieter;
            Bezeichnung = p.Bezeichnung;
        }
    }

    public sealed class VertragDetailWohnung
    {
        public override string ToString() => BezeichnungVoll;
        public Wohnung Entity { get; }

        public int Id { get; }
        public string Besitzer { get; }
        public string BezeichnungVoll { get; }

        public VertragDetailWohnung(Wohnung w)
        {
            Entity = w;
            Id = w.WohnungId;
            Besitzer = App.Walter.FindPerson(w.BesitzerId)?.Bezeichnung;
            BezeichnungVoll = AdresseViewModel.Anschrift(w) + ", " + w.Bezeichnung;
        }

        public static Wohnung GetWohnung(int id) => App.Walter.Wohnungen.Find(id);
    }
}
