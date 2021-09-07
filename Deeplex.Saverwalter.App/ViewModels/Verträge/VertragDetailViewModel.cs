using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragDetailViewModel : VertragDetailVersion
    {
        public Guid guid { get; }
        public ObservableProperty<ImmutableList<KontaktListEntry>> AlleMieter
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();
        public ObservableProperty<KontaktListEntry> AddMieter = new ObservableProperty<KontaktListEntry>();

        public void UpdateMieterList()
        {
            AlleMieter.Value = Impl.ctx.JuristischePersonen
                    .ToImmutableList()
                    .Where(j => j.isMieter == true).Select(j => new KontaktListEntry(j))
                    .Concat(Impl.ctx.NatuerlichePersonen
                        .Where(n => n.isMieter == true).Select(n => new KontaktListEntry(n)))
                    .Where(p => !Mieter.Value.Exists(e => p.Guid == e.Guid))
                    .ToImmutableList();
        }

        public List<WohnungListEntry> AlleWohnungen = new List<WohnungListEntry>();
        public List<KontaktListEntry> AlleKontakte;

        public ObservableProperty<ImmutableList<VertragDetailVersion>> Versionen
            = new ObservableProperty<ImmutableList<VertragDetailVersion>>();

        public ObservableProperty<ImmutableList<KontaktListEntry>> Mieter
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();
        public DateTimeOffset? AddVersionDatum;
        public ObservableProperty<int> BetriebskostenJahr
            = new ObservableProperty<int>();

        public string lastBeginn
        {
            get => Versionen.Value.Last().Beginn.ToString("dd.MM.yyyy");
        }
        public string firstEnde
        {
            get => Versionen.Value.First().Ende is DateTimeOffset d ? d.ToString("dd.MM.yyyy") : "Offen";
        }
        public int StartJahr => Versionen.Value.Last().Beginn.Year;
        public int EndeJahr => Versionen.Value.First().Ende?.Year ?? 9999;

        public VertragDetailViewModel(IAppImplementation impl) : this(new List<Vertrag>
            { new Vertrag { Beginn = DateTime.UtcNow.Date, } }, impl)
        { }

        public VertragDetailViewModel(Guid id, IAppImplementation impl)
            : this(impl.ctx.Vertraege
                  .Where(v => v.VertragId == id)
                  .Include(v => v.Wohnung)
                  .ToList()
                  .OrderBy(v => v.Version)
                  .Reverse()
                  .ToList(), impl)
        { }

        public VertragDetailViewModel(List<Vertrag> v, IAppImplementation impl) : base(v.OrderBy(vs => vs.Version).Last(), impl)
        {
            guid = v.First().VertragId;

            Versionen.Value = v.Select(vs => new VertragDetailVersion(vs, Impl)).ToImmutableList();

            AlleWohnungen = Impl.ctx.Wohnungen.Select(w => new WohnungListEntry(w, Impl)).ToList();
            Wohnung = AlleWohnungen.Find(w => w.Id == v.First().WohnungId);

            AlleKontakte = Impl.ctx.JuristischePersonen.ToList().Select(j => new KontaktListEntry(j))
                    .Concat(Impl.ctx.NatuerlichePersonen.Select(n => new KontaktListEntry(n)))
                    .ToList();
            Ansprechpartner = AlleKontakte.Find(w => w.Guid == v.First().AnsprechpartnerId);

            Mieter.Value = Impl.ctx.MieterSet
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new KontaktListEntry(m.PersonId, Impl))
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
                var nv = new VertragDetailVersion(entity, Impl);
                Versionen.Value = Versionen.Value.Insert(0, nv);
                Impl.ctx.Vertraege.Add(entity);
                Impl.SaveWalter();
            }, _ => true);

            RemoveVersion = new AsyncRelayCommand(async _ =>
            {
                if (await Impl.Confirmation())
                {
                    var vs = Versionen.Value.First().Entity;
                    Impl.ctx.Vertraege.Remove(vs);
                    Versionen.Value = Versionen.Value.Skip(1).ToImmutableList();
                    Impl.SaveWalter();
                }
            }, _ => true);
        }

        public RelayCommand AddMiete { get; }
        public RelayCommand AddMietMinderung { get; }
        public RelayCommand AddVersion { get; }
        public AsyncRelayCommand RemoveVersion { get; }

        public async void SelfDestruct()
        {
            Versionen.Value.ForEach(v =>
            {
                Impl.ctx.Mieten
                    .Where(m => m.VertragId == guid)
                    .ToList()
                    .ForEach(m => Impl.ctx.Mieten.Remove(m));
                Impl.ctx.MietMinderungen
                    .Where(m => m.VertragId == guid)
                    .ToList()
                    .ForEach(m => Impl.ctx.MietMinderungen.Remove(m));

                Impl.ctx.Vertraege.Remove(v.Entity);
            });
            Impl.SaveWalter();
        }
    }
}
