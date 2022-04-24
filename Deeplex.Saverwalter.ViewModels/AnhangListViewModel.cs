using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    // TODO i18n out of viewmodels...
    public sealed class AnhangListViewModel : BindableBase
    {
        public ObservableProperty<string> Text = new();
        public ObservableProperty<ImmutableList<AnhangListViewModelEntry>> Liste = new();

        public IAppImplementation Impl;
        public IWalterDbService Db;

        private void SetList<T>(T a, IQueryable<IAnhang<T>> set)
        {
            var self = this;

            Liste.Value = set.Include(e => e.Anhang)
                .ToList()
                .Where(b => Equals(b.Target, a))
                .ToList()
                .Select(e => new AnhangListViewModelEntry(e, self))
                .ToImmutableList();
        }

        public RelayCommand AddAnhang;

        private void Initialize(string text, IAppImplementation impl, IWalterDbService db)
        {
            Db = db;
            Impl = impl;
            Text.Value = text;
        }

        public AnhangListViewModel(IAppImplementation impl, IWalterDbService db)
        {
            Initialize("Anhänge", impl, db);
            var self = this;
            Liste.Value = Db.ctx.Anhaenge.Select(a => new AnhangListViewModelEntry(a, self)).ToImmutableList();
        }

        public AnhangListViewModel(Adresse a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(AdresseViewModel.Anschrift(a), impl, db);
            SetList(a, db.ctx.AdresseAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.AdresseAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Betriebskostenrechnung a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(a.BetreffendesJahr.ToString() + ", " + a.Schluessel.ToDescriptionString(), impl, db);
            SetList(a, db.ctx.BetriebskostenrechnungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.BetriebskostenrechnungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Erhaltungsaufwendung a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(a.Bezeichnung, impl, db);
            SetList(a, db.ctx.ErhaltungsaufwendungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.ErhaltungsaufwendungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Garage a, IAppImplementation impl, IWalterDbService avm)
        {
            Initialize(AdresseViewModel.Anschrift(a.Adresse) + ", " + a.Kennung, impl, avm);
            SetList(a, avm.ctx.GarageAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(avm.ctx.GarageAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(JuristischePerson a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(a.Bezeichnung, impl, db);
            SetList(a, db.ctx.JuristischePersonAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.JuristischePersonAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Konto a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(a.Iban, impl, db);
            SetList(a, db.ctx.KontoAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.KontoAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Miete a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize("Miete: " + a.BetreffenderMonat.ToString("mm.yyyy"), impl, db);
            SetList(a, db.ctx.MieteAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.MieteAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(MietMinderung a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize("Mietminderung", impl, db);
            SetList(a, db.ctx.MietMinderungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.MietMinderungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(NatuerlichePerson a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(a.Bezeichnung, impl, db);
            SetList(a, db.ctx.NatuerlichePersonAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.NatuerlichePersonAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Vertrag a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(a.Wohnung != null ?
                "Vertrag: " + AdresseViewModel.Anschrift(a.Wohnung) + ", " + a.Wohnung.Bezeichnung :
                "Keine Wohnung", impl, db);
            SetList(a.VertragId, db.ctx.VertragAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.VertragAnhaenge, a.VertragId, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Wohnung a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(AdresseViewModel.Anschrift(a) + ", " + a.Bezeichnung, impl, db);
            SetList(a, db.ctx.WohnungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.WohnungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Zaehler a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(a.Kennnummer, impl, db);
            SetList(a, db.ctx.ZaehlerAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.ZaehlerAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Zaehlerstand a, IAppImplementation impl, IWalterDbService db)
        {
            Initialize(a.Datum.ToString("dd.MM.yyyy") + ": " + a.Stand.ToString(), impl, db);
            SetList(a, db.ctx.ZaehlerstandAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.ZaehlerstandAnhaenge, a, f as List<Anhang>), _ => true);
        }

        public async void SaveAnhang<T, U>(DbSet<T> Set, U target, List<Anhang> newFiles = null)
            where T : class, IAnhang<U>, new()
        {
            if (newFiles == null)
            {
                newFiles = (await Impl.pickFiles()).Select(f => Utils.Files.SaveAnhang(f, Db.root)).ToList();
            }
            Utils.Files.ConnectAnhangToEntity(Set, target, newFiles, Impl, Db);
            var self = this;
            newFiles.ForEach(f => Liste.Value = Liste.Value.Add(new AnhangListViewModelEntry(f, self)));
        }
    }
}
