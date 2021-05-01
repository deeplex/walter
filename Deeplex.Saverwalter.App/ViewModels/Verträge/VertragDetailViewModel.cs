using Deeplex.Saverwalter.Model;
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
        public ObservableProperty<ImmutableList<KontaktListEntry>> AlleMieter
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();

        public void UpdateMieterList()
        {
            AlleMieter.Value = App.Walter.JuristischePersonen.ToImmutableList()
                    .Where(j => j.isMieter == true).Select(j => new KontaktListEntry(j))
                    .Concat(App.Walter.NatuerlichePersonen
                        .Where(n => n.isMieter == true).Select(n => new KontaktListEntry(n)))
                    .Where(p => !Mieter.Value.Exists(e => p.Id == e.Id))
                    .ToImmutableList();
        }

        public List<WohnungListEntry> AlleWohnungen = new List<WohnungListEntry>();

        public ImmutableList<KontaktListEntry> AlleKontakte =>
                App.Walter.JuristischePersonen.ToImmutableList().Select(j => new KontaktListEntry(j))
                    .Concat(App.Walter.NatuerlichePersonen.Select(n => new KontaktListEntry(n)))
                    .ToImmutableList();

        public ObservableProperty<ImmutableList<VertragDetailVersion>> Versionen
            = new ObservableProperty<ImmutableList<VertragDetailVersion>>();

        public ObservableProperty<ImmutableList<KontaktListEntry>> Mieter
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();
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

            Versionen.Value = v.Select(vs => new VertragDetailVersion(vs)).ToImmutableList();

            AlleWohnungen = App.Walter.Wohnungen.Select(w => new WohnungListEntry(w)).ToList();
            Wohnung = AlleWohnungen.Find(w => w.Id == v.First().WohnungId);

            Mieter.Value = App.Walter.MieterSet
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new KontaktListEntry(m.PersonId))
                .ToImmutableList();

            UpdateMieterList();

            BetriebskostenJahr.Value = DateTime.Now.Year - 1;

            AddVersion = new RelayCommand(_ =>
            {
                var last = Versionen.Value.First().Entity;
                var entity = new Vertrag(last, AddVersionDatum?.UtcDateTime ?? DateTime.UtcNow.Date)
                {
                    Personenzahl = Personenzahl,
                    //KaltMiete = KaltMiete, TODO
                };
                var nv = new VertragDetailVersion(entity);
                Versionen.Value = Versionen.Value.Insert(0, nv);
                App.Walter.Vertraege.Add(entity);
                App.SaveWalter();
            }, _ => true);

            RemoveVersion = new RelayCommand(_ =>
            {
                var vs = Versionen.Value.First().Entity;
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

        public void SelfDestruct()
        {
            Versionen.Value.ForEach(v =>
            {
                App.Walter.Mieten
                    .Where(m => m.VertragId == guid)
                    .ToList()
                    .ForEach(m => App.Walter.Mieten.Remove(m));
                App.Walter.MietMinderungen
                    .Where(m => m.VertragId == guid)
                    .ToList()
                    .ForEach(m => App.Walter.MietMinderungen.Remove(m));

                App.Walter.Vertraege.Remove(v.Entity);
            });
            App.SaveWalter();
        }
    }
}
