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
    // TODO i18n out of viewmodels...
    public sealed class AnhangListViewModel : BindableBase
    {
        public ObservableProperty<string> Text = new();
        public ObservableProperty<ImmutableList<AnhangListViewModelEntry>> Liste = new();

        public IFileService fs;
        public IWalterDbService Db;
        public INotificationService NotificationService;

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

        private void Initialize(string text, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Db = db;
            this.fs = fs;
            Text.Value = text;
        }

        public AnhangListViewModel(string text, ImmutableList<AnhangListViewModelEntry> liste, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Text.Value = text;
            Liste.Value = liste;
            this.fs = fs;
            Db = db;
            NotificationService = ns;
        }

        public static AnhangListViewModel create<T>(T entity, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            if (entity is Adresse a)
            {
                return new AnhangListViewModel(a, fs, ns, db);
            }
            else if (entity is Betriebskostenrechnung b)
            {
                return new AnhangListViewModel(b, fs, ns, db);
            }
            else if (entity is Erhaltungsaufwendung e)
            {
                return new AnhangListViewModel(e, fs, ns, db);
            }
            else if (entity is Garage g)
            {
                return new AnhangListViewModel(g, fs, ns, db);
            }
            else if (entity is JuristischePerson j)
            {
                return new AnhangListViewModel(j, fs, ns, db);
            }
            else if (entity is Konto k)
            {
                return new AnhangListViewModel(k, fs, ns, db);
            }
            else if (entity is Miete m)
            {
                return new AnhangListViewModel(m, fs, ns, db);
            }
            else if (entity is MietMinderung mm)
            {
                return new AnhangListViewModel(mm, fs, ns, db);
            }
            else if (entity is NatuerlichePerson n)
            {
                return new AnhangListViewModel(n, fs, ns, db);
            }
            else if (entity is Guid guid && db.ctx.Vertraege.FirstOrDefault(e => e.VertragId == guid) is Vertrag v)
            {
                return new AnhangListViewModel(v, fs, ns, db);
            }
            else if (entity is Wohnung w)
            {
                return new AnhangListViewModel(w, fs, ns, db);
            }
            else if (entity is Zaehler z)
            {
                return new AnhangListViewModel(z, fs, ns, db);
            }
            else if (entity is Zaehlerstand zs)
            {
                return new AnhangListViewModel(zs, fs, ns, db);
            }
            return null;
        }

        public AnhangListViewModel(IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize("Anhänge", fs, ns, db);
            var self = this;
            Liste.Value = Db.ctx.Anhaenge.Select(a => new AnhangListViewModelEntry(a, self)).ToImmutableList();
        }

        public AnhangListViewModel(Adresse a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(AdresseViewModel.Anschrift(a), fs, ns, db);
            SetList(a, db.ctx.AdresseAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.AdresseAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Betriebskostenrechnung a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(a.BetreffendesJahr.ToString() + ", " + a.Schluessel.ToDescriptionString(), fs, ns, db);
            SetList(a, db.ctx.BetriebskostenrechnungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.BetriebskostenrechnungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Erhaltungsaufwendung a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(a.Bezeichnung, fs, ns, db);
            SetList(a, db.ctx.ErhaltungsaufwendungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.ErhaltungsaufwendungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Garage a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(AdresseViewModel.Anschrift(a.Adresse) + ", " + a.Kennung, fs, ns, db);
            SetList(a, db.ctx.GarageAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.GarageAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(JuristischePerson a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(a.Bezeichnung, fs, ns, db);
            SetList(a, db.ctx.JuristischePersonAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.JuristischePersonAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Konto a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(a.Iban, fs, ns, db);
            SetList(a, db.ctx.KontoAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.KontoAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Miete a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize("Miete: " + a.BetreffenderMonat.ToString("mm.yyyy"), fs, ns, db);
            SetList(a, db.ctx.MieteAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.MieteAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(MietMinderung a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize("Mietminderung", fs, ns, db);
            SetList(a, db.ctx.MietMinderungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.MietMinderungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(NatuerlichePerson a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(a.Bezeichnung, fs, ns, db);
            SetList(a, db.ctx.NatuerlichePersonAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.NatuerlichePersonAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Vertrag a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(a.Wohnung != null ?
                "Vertrag: " + AdresseViewModel.Anschrift(a.Wohnung) + ", " + a.Wohnung.Bezeichnung :
                "Keine Wohnung", fs, ns, db);
            SetList(a.VertragId, db.ctx.VertragAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.VertragAnhaenge, a.VertragId, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Wohnung a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(AdresseViewModel.Anschrift(a) + ", " + a.Bezeichnung, fs, ns, db);
            SetList(a, db.ctx.WohnungAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.WohnungAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Zaehler a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(a.Kennnummer, fs, ns, db);
            SetList(a, db.ctx.ZaehlerAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.ZaehlerAnhaenge, a, f as List<Anhang>), _ => true);
        }
        public AnhangListViewModel(Zaehlerstand a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Initialize(a.Datum.ToString("dd.MM.yyyy") + ": " + a.Stand.ToString(), fs, ns, db);
            SetList(a, db.ctx.ZaehlerstandAnhaenge);
            AddAnhang = new RelayCommand(f => SaveAnhang(db.ctx.ZaehlerstandAnhaenge, a, f as List<Anhang>), _ => true);
        }

        public async void SaveAnhang<T, U>(DbSet<T> Set, U target, List<Anhang> newFiles = null)
            where T : class, IAnhang<U>, new()
        {
            if (newFiles == null)
            {
                newFiles = (await fs.pickFiles()).Select(f => Files.SaveAnhang(f, Db.root)).ToList();
            }
            Files.ConnectAnhangToEntity(Set, target, newFiles, Db);
            var self = this;
            newFiles.ForEach(f => Liste.Value = Liste.Value.Add(new AnhangListViewModelEntry(f, self)));
        }
    }
}
