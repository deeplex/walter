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
    // TODO i18n out of viewmodels...
    public sealed class AnhangListViewModel : BindableBase
    {
        public ObservableProperty<string> Text = new ObservableProperty<string>();
        public ObservableProperty<ImmutableList<AnhangListEntry>> Liste =
            new ObservableProperty<ImmutableList<AnhangListEntry>>();

        public IAppImplementation Impl;
        public AppViewModel Avm;

        private void SetList<T>(T a, IQueryable<IAnhang<T>> set)
        {
            var self = this;

            Liste.Value = set.Include(e => e.Anhang)
                .ToList()
                .Where(b => Equals(b.Target, a))
                .ToList()
                .Select(e => new AnhangListEntry(e, self))
                .ToImmutableList();
        }

        public RelayCommand AddAnhang;

        private void Initialize(string text, IAppImplementation impl, AppViewModel avm)
        {
            Avm = avm;
            Impl = impl;
            Text.Value = text;
        }

        public AnhangListViewModel(IAppImplementation impl, AppViewModel avm)
        {
            Initialize("Anhänge", impl, avm);
            var self = this;
            Liste.Value = Avm.ctx.Anhaenge.Select(a => new AnhangListEntry(a, self)).ToImmutableList();
        }

        public AnhangListViewModel(Adresse a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(AdresseViewModel.Anschrift(a), impl, avm);
            SetList(a, avm.ctx.AdresseAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.AdresseAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Betriebskostenrechnung a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(a.BetreffendesJahr.ToString() + ", " + a.Schluessel.ToDescriptionString(), impl, avm);
            SetList(a, avm.ctx.BetriebskostenrechnungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.BetriebskostenrechnungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Erhaltungsaufwendung a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(a.Bezeichnung, impl, avm);
            SetList(a, avm.ctx.ErhaltungsaufwendungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.ErhaltungsaufwendungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Garage a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(AdresseViewModel.Anschrift(a.Adresse) + ", " + a.Kennung, impl, avm);
            SetList(a, avm.ctx.GarageAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.GarageAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(JuristischePerson a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(a.Bezeichnung, impl, avm);
            SetList(a, avm.ctx.JuristischePersonAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.JuristischePersonAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Konto a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(a.Iban, impl, avm);
            SetList(a, avm.ctx.KontoAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.KontoAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Miete a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize("Miete: " + a.BetreffenderMonat.ToString("mm.yyyy"), impl, avm);
            SetList(a, avm.ctx.MieteAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.MieteAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(MietMinderung a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize("Mietminderung", impl, avm);
            SetList(a, avm.ctx.MietMinderungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.MietMinderungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(NatuerlichePerson a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(a.Bezeichnung, impl, avm);
            SetList(a, avm.ctx.NatuerlichePersonAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.NatuerlichePersonAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Vertrag a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(a.Wohnung != null ?
                "Vertrag: " + AdresseViewModel.Anschrift(a.Wohnung) + ", " + a.Wohnung.Bezeichnung :
                "Keine Wohnung", impl, avm);
            SetList(a.VertragId, avm.ctx.VertragAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.VertragAnhaenge, a.VertragId, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Wohnung a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(AdresseViewModel.Anschrift(a) + ", " + a.Bezeichnung, impl, avm);
            SetList(a, avm.ctx.WohnungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.WohnungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Zaehler a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(a.Kennnummer, impl, avm);
            SetList(a, avm.ctx.ZaehlerAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.ZaehlerAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Zaehlerstand a, IAppImplementation impl, AppViewModel avm)
        {
            Initialize(a.Datum.ToString("dd.MM.yyyy") + ": " + a.Stand.ToString(), impl, avm);
            SetList(a, avm.ctx.ZaehlerstandAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.ZaehlerstandAnhaenge, a, f as List<Anhang>), _ => true);
        }

        public async void SaveAnhang<T, U>(DbSet<T> Set, U target, List<Anhang> newFiles = null)
            where T : class, IAnhang<U>, new()
        {
            if (newFiles == null)
            {
                newFiles = (await Impl.pickFiles()).Select(f => Utils.Files.SaveAnhang(f, Avm.root)).ToList();
            }
            Utils.Files.ConnectAnhangToEntity(Set, target, newFiles, Impl, Avm);
            var self = this;
            newFiles.ForEach(f => Liste.Value = Liste.Value.Add(new AnhangListEntry(f, self)));
        }
    }

    public sealed class AnhangListEntry
    {
        public Anhang Entity { get; }
        public override string ToString() => Entity.FileName;
        public DateTime CreationTime => Entity.CreationTime;

        public int GetReferences
        {
            get
            {
                var count = Container.Avm.ctx.AdresseAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.BetriebskostenrechnungAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.ErhaltungsaufwendungAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.GarageAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.JuristischePersonAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.KontoAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.MieteAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.MietMinderungAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.NatuerlichePersonAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.VertragAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.WohnungAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.ZaehlerAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.ZaehlerstandAnhaenge.Count(j => j.Anhang == Entity);

                return count;
            }
        }

        public string path => Entity.getPath(Container.Avm.root);
        public double size => File.Exists(path) ? new FileInfo(path).Length : 0;

        public AnhangListViewModel Container { get; }

        public AnhangListEntry(IAnhang a, AnhangListViewModel vm) : this(a.Anhang, vm) { }
        public AnhangListEntry(Anhang a, AnhangListViewModel vm)
        {
            Container = vm;
            Entity = a;
        }

        public async void DeleteFile()
        {
            try
            {
                if (await Container.Impl.Confirmation())
                {
                    Container.Avm.ctx.Anhaenge.Remove(Entity);
                    Container.Avm.SaveWalter();

                    File.Delete(Entity.getPath(Container.Avm.root));

                    var deleted = Container.Liste.Value.Find(e => e.Entity.AnhangId == Entity.AnhangId);
                    if (deleted != null)
                    {
                        Container.Liste.Value = Container.Liste.Value.Remove(deleted);
                    }
                }
            }
            catch (Exception e)
            {
                Container.Impl.ShowAlert(e.Message);
            }
        }

        public void OpenFile()
        {
            try
            {
                Container.Impl.launchFile(Entity);
            }
            catch (Exception e)
            {
                Container.Impl.ShowAlert(e.Message);
            }

        }

        public string DateiName => Entity.FileName;
    }
}
