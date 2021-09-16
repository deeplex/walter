using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class AnhangListViewModel : BindableBase
    {
        public ObservableProperty<string> Text = new ObservableProperty<string>();
        public ObservableProperty<ImmutableList<AnhangListEntry>> Liste =
            new ObservableProperty<ImmutableList<AnhangListEntry>>();

        private IAppImplementation Impl;

        private void SetList<T>(T a, IQueryable<IAnhang<T>> set)
        {
            var self = this;

            Liste.Value = set.Include(e => e.Anhang)
                .ToList()
                .Where(b => Equals(b.Target, a))
                .ToList()
                .Select(e => new AnhangListEntry(e, self, Impl))
                .ToImmutableList();

            if (!Liste.Value.IsEmpty)
            {
                Impl.OpenAnhangPane();
            }
        }

        public RelayCommand AddAnhang;

        private void Initialize(string text, IAppImplementation impl)
        {
            Impl = impl;
            Text.Value = text;
        }

        public AnhangListViewModel(IAppImplementation impl)
        {
            Impl = impl;
            Text.Value = "Anhänge";
            var self = this;
            Liste.Value = impl.ctx.Anhaenge.Select(a => new AnhangListEntry(a, self, impl)).ToImmutableList();
        }

        public AnhangListViewModel(Adresse a, IAppImplementation impl)
        {
            Initialize(AdresseViewModel.Anschrift(a), impl);
            SetList(a, Impl.ctx.AdresseAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.AdresseAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Betriebskostenrechnung a, IAppImplementation impl)
        {
            Initialize(a.BetreffendesJahr.ToString() + ", " + a.Schluessel.ToDescriptionString(), impl);
            SetList(a, Impl.ctx.BetriebskostenrechnungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.BetriebskostenrechnungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Erhaltungsaufwendung a, IAppImplementation impl)
        {
            Initialize(a.Bezeichnung, impl);
            SetList(a, Impl.ctx.ErhaltungsaufwendungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.ErhaltungsaufwendungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Garage a, IAppImplementation impl)
        {
            Initialize(AdresseViewModel.Anschrift(a.Adresse) + ", " + a.Kennung, impl);
            SetList(a, Impl.ctx.GarageAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.GarageAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(JuristischePerson a, IAppImplementation impl)
        {
            Initialize(a.Bezeichnung, impl);
            SetList(a, Impl.ctx.JuristischePersonAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.JuristischePersonAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Konto a, IAppImplementation impl)
        {
            Initialize(a.Iban, impl);
            SetList(a, Impl.ctx.KontoAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.KontoAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Miete a, IAppImplementation impl)
        {
            Initialize("Miete: " + a.BetreffenderMonat.ToString("mm.yyyy"), impl);
            SetList(a, Impl.ctx.MieteAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.MieteAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(MietMinderung a, IAppImplementation impl)
        {
            Initialize("Mietminderung", impl);
            SetList(a, Impl.ctx.MietMinderungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.MietMinderungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(NatuerlichePerson a, IAppImplementation impl)
        {
            Initialize(a.Bezeichnung, impl);
            SetList(a, Impl.ctx.NatuerlichePersonAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.NatuerlichePersonAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Vertrag a, IAppImplementation impl)
        {
            Initialize(a.Wohnung != null ?
                "Vertrag: " + AdresseViewModel.Anschrift(a.Wohnung) + ", " + a.Wohnung.Bezeichnung :
                "Keine Wohnung", impl);
            SetList(a.VertragId, Impl.ctx.VertragAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.VertragAnhaenge, a.VertragId, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Wohnung a, IAppImplementation impl)
        {
            Initialize(AdresseViewModel.Anschrift(a) + ", " + a.Bezeichnung, impl);
            SetList(a, Impl.ctx.WohnungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.WohnungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Zaehler a, IAppImplementation impl)
        {
            Initialize(a.Kennnummer, impl);
            SetList(a, Impl.ctx.ZaehlerAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.ZaehlerAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Zaehlerstand a, IAppImplementation impl)
        {
            Initialize(a.Datum.ToString("dd.MM.yyyy") + ": " + a.Stand.ToString(), impl);
            SetList(a, Impl.ctx.ZaehlerstandAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(Impl.ctx.ZaehlerstandAnhaenge, a, f as List<Anhang>), _ => true);
        }

        public async void SaveAnhang<T, U>(DbSet<T> Set, U target, List<Anhang> newFiles = null)
            where T : class, IAnhang<U>, new()
        {
            if (newFiles == null)
            {
                newFiles = (await Impl.pickFiles()).Select(f => Utils.Files.SaveAnhang(f, Impl.root)).ToList();
            }
            Utils.Files.ConnectAnhangToEntity(Set, target, newFiles, Impl);
            var self = this;
            newFiles.ForEach(f => Liste.Value = Liste.Value.Add(new AnhangListEntry(f, self, Impl)));
        }
    }

    public sealed class AnhangListEntry
    {
        public Anhang Entity { get; }
        public override string ToString() => Entity.FileName;
        public string CreatedString => "Erstellt am: " + Entity.CreationTime.ToString("dd.MM.yyyy HH:mm:ss");
        public string FileNameString => "Dateipfad: " + path;
        public string FileSizeString => "Dateigröße: " + Math.Round(size / 1000).ToString() + "kb";

        public int GetReferences
        {
            get
            {
                var count = Impl.ctx.AdresseAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.BetriebskostenrechnungAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.ErhaltungsaufwendungAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.GarageAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.JuristischePersonAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.KontoAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.MieteAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.MietMinderungAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.NatuerlichePersonAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.VertragAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.WohnungAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.ZaehlerAnhaenge.Count(j => j.Anhang == Entity);
                count += Impl.ctx.ZaehlerstandAnhaenge.Count(j => j.Anhang == Entity);

                return count;
            }
        }

        private string path => Entity.getPath(Impl.root);
        public double size => File.Exists(path) ? new FileInfo(path).Length : 0;

        public AnhangListViewModel Container { get; }

        private IAppImplementation Impl;

        public AnhangListEntry(Anhang a, AnhangListViewModel vm, IAppImplementation impl)
        {
            Container = vm;
            Entity = a;
            Impl = impl;
        }
        public AnhangListEntry(IAnhang a, AnhangListViewModel vm, IAppImplementation impl) : this(a.Anhang, vm, impl) { }

        public async void DeleteFile()
        {
            try
            {
                if (await Impl.Confirmation())
                {
                    Impl.ctx.Anhaenge.Remove(Entity);
                    Impl.SaveWalter();

                    File.Delete(Entity.getPath(Impl.root));

                    var deleted = Container.Liste.Value.Find(e => e.Entity.AnhangId == Entity.AnhangId);
                    if (deleted != null)
                    {
                        Container.Liste.Value = Container.Liste.Value.Remove(deleted);
                    }
                }
            }
            catch (Exception e)
            {
                Impl.ShowAlert(e.Message);
            }
        }

        public void OpenFile()
        {
            try
            {
                Impl.launchFile(Entity);
            }
            catch (Exception e)
            {
                Impl.ShowAlert(e.Message);
            }

        }

        public string DateiName => Entity.FileName;
    }
}
