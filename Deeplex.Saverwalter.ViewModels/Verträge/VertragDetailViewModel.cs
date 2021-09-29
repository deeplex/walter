using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

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
            AlleMieter.Value = Avm.ctx.JuristischePersonen
                    .ToImmutableList()
                    .Where(j => j.isMieter == true).Select(j => new KontaktListEntry(j))
                    .Concat(Avm.ctx.NatuerlichePersonen
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

        public DateTimeOffset lastBeginn => Versionen.Value.Last().Beginn;
        public DateTimeOffset? firstEnde => Versionen.Value.First().Ende;
        public int StartJahr => Versionen.Value.Last().Beginn.Year;
        public int EndeJahr => Versionen.Value.First().Ende?.Year ?? 9999;

        public VertragDetailViewModel(IAppImplementation impl, AppViewModel avm) : this(new List<Vertrag>
            { new Vertrag { Beginn = DateTime.UtcNow.Date, } }, impl, avm)
        { }

        public VertragDetailViewModel(Guid id, IAppImplementation impl, AppViewModel avm)
            : this(avm.ctx.Vertraege
                  .Where(v => v.VertragId == id)
                  .Include(v => v.Wohnung)
                  .ToList()
                  .OrderBy(v => v.Version)
                  .Reverse()
                  .ToList(), impl, avm)
        { }

        public VertragDetailViewModel(List<Vertrag> v, IAppImplementation impl, AppViewModel avm) : base(v.OrderBy(vs => vs.Version).Last(), impl, avm)
        {
            guid = v.First().VertragId;

            Versionen.Value = v.Select(vs => new VertragDetailVersion(vs, impl, avm)).ToImmutableList();

            AlleWohnungen = avm.ctx.Wohnungen.Select(w => new WohnungListEntry(w, avm)).ToList();
            Wohnung = AlleWohnungen.Find(w => w.Id == v.First().WohnungId);

            AlleKontakte = avm.ctx.JuristischePersonen.ToList().Select(j => new KontaktListEntry(j))
                    .Concat(avm.ctx.NatuerlichePersonen.Select(n => new KontaktListEntry(n)))
                    .ToList();
            Ansprechpartner = AlleKontakte.Find(w => w.Guid == v.First().AnsprechpartnerId);

            Mieter.Value = avm.ctx.MieterSet
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new KontaktListEntry(m.PersonId, avm))
                .ToImmutableList();

            UpdateMieterList();

            BetriebskostenJahr.Value = DateTime.Now.Year - 1;

            AddMieterCommand = new RelayCommand(_ =>
            {
                if (AddMieter.Value?.Guid is Guid guid)
                {
                    Mieter.Value = Mieter.Value.Add(new KontaktListEntry(guid, Avm));
                    UpdateMieterList();
                    Avm.ctx.MieterSet.Add(new Mieter()
                    {
                        VertragId = guid,
                        PersonId = guid,
                    });
                    Avm.SaveWalter();
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
                var nv = new VertragDetailVersion(entity, impl, avm);
                Versionen.Value = Versionen.Value.Insert(0, nv);
                avm.ctx.Vertraege.Add(entity);
                avm.SaveWalter();
            }, _ => true);

            RemoveVersion = new AsyncRelayCommand(async _ =>
            {
                if (await impl.Confirmation())
                {
                    var vs = Versionen.Value.First().Entity;
                    avm.ctx.Vertraege.Remove(vs);
                    Versionen.Value = Versionen.Value.Skip(1).ToImmutableList();
                    avm.SaveWalter();
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
                    Avm.ctx.Mieten
                        .Where(m => m.VertragId == guid)
                        .ToList()
                        .ForEach(m => Avm.ctx.Mieten.Remove(m));
                    Avm.ctx.MietMinderungen
                        .Where(m => m.VertragId == guid)
                        .ToList()
                        .ForEach(m => Avm.ctx.MietMinderungen.Remove(m));

                    Avm.ctx.Vertraege.Remove(v.Entity);
                });
                Avm.SaveWalter();
            }

        }
    }
}
