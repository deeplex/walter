using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels.Utils;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class AnhangListViewModel : BindableBase
    {
        public ObservableProperty<string> Text = new ObservableProperty<string>();
        public ObservableProperty<ImmutableList<AnhangListEntry>> Liste =
            new ObservableProperty<ImmutableList<AnhangListEntry>>();

        private IAppImplementation Impl;

        public void AddAnhangToList(Anhang a)
        {
            var self = this;
            Liste.Value = Liste.Value.Add(new AnhangListEntry(a, self, Impl));
        }

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
                Impl.OpenAnhang();
            }
        }

        private ObservableProperty<ImmutableList<Anhang>> newFiles
            = new ObservableProperty<ImmutableList<Anhang>>();

        public void DropFile(Anhang a)
        {
            if (newFiles.Value == null)
            {
                newFiles.Value = ImmutableList.Create<Anhang>();
            }
            newFiles.Value = newFiles.Value.Add(a);
        }

        public AsyncRelayCommand AddAnhang;

        private void Initialize(string text, IAppImplementation impl)
        {
            Impl = impl;
            Text.Value = text;
        }

        public AnhangListViewModel(Adresse a, IAppImplementation impl)
        {
            Initialize(AdresseViewModel.Anschrift(a), impl);
            SetList(a, Impl.ctx.AdresseAnhaenge);
            AddAnhang = new AsyncRelayCommand(async files =>
                await PickFilesAndSaveToWalter(Impl.ctx.AdresseAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Betriebskostenrechnung a, IAppImplementation impl)
        {
            Initialize(a.BetreffendesJahr.ToString() + ", " + a.Schluessel.ToDescriptionString(), impl);
            SetList(a, Impl.ctx.BetriebskostenrechnungAnhaenge);
            AddAnhang = new AsyncRelayCommand(async files =>
                await PickFilesAndSaveToWalter(Impl.ctx.BetriebskostenrechnungAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Erhaltungsaufwendung a, IAppImplementation impl)
        {
            Initialize(a.Bezeichnung, impl);
            SetList(a, Impl.ctx.ErhaltungsaufwendungAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.ErhaltungsaufwendungAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Garage a, IAppImplementation impl)
        {
            Initialize(AdresseViewModel.Anschrift(a.Adresse) + ", " + a.Kennung, impl);
            SetList(a, Impl.ctx.GarageAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.GarageAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(JuristischePerson a, IAppImplementation impl)
        {
            Initialize(a.Bezeichnung, impl);
            SetList(a, Impl.ctx.JuristischePersonAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.JuristischePersonAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Konto a, IAppImplementation impl)
        {
            Initialize(a.Iban, impl);
            SetList(a, Impl.ctx.KontoAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.KontoAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Miete a, IAppImplementation impl)
        {
            Initialize("Miete: " + a.BetreffenderMonat.ToString("mm.yyyy"), impl);
            SetList(a, Impl.ctx.MieteAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.MieteAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(MietMinderung a, IAppImplementation impl)
        {
            Initialize("Mietminderung", impl);
            SetList(a, Impl.ctx.MietMinderungAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.MietMinderungAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(NatuerlichePerson a, IAppImplementation impl)
        {
            Initialize(a.Bezeichnung, impl);
            SetList(a, Impl.ctx.NatuerlichePersonAnhaenge);
            AddAnhang = new AsyncRelayCommand(async files =>
                await PickFilesAndSaveToWalter(Impl.ctx.NatuerlichePersonAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Vertrag a, IAppImplementation impl)
        {
            Initialize(a.Wohnung != null ?
                "Vertrag: " + AdresseViewModel.Anschrift(a.Wohnung) + ", " + a.Wohnung.Bezeichnung :
                "Keine Wohnung", impl);
            SetList(a.VertragId, Impl.ctx.VertragAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.VertragAnhaenge, a.VertragId),
                _ => true);
        }
        public AnhangListViewModel(Wohnung a, IAppImplementation impl)
        {
            Initialize(AdresseViewModel.Anschrift(a) + ", " + a.Bezeichnung, impl);
            SetList(a, Impl.ctx.WohnungAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.WohnungAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Zaehler a, IAppImplementation impl)
        {
            Initialize(a.Kennnummer, impl);
            SetList(a, Impl.ctx.ZaehlerAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.ZaehlerAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Zaehlerstand a, IAppImplementation impl)
        {
            Initialize(a.Datum.ToString("dd.MM.yyyy") + ": " + a.Stand.ToString(), impl);
            SetList(a, Impl.ctx.ZaehlerstandAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await PickFilesAndSaveToWalter(Impl.ctx.ZaehlerstandAnhaenge, a),
                _ => true);
        }

        public async Task PickFilesAndSaveToWalter<T, U>(DbSet<T> Set, U target) where T : class, IAnhang<U>, new()
        {
            if (newFiles.Value == null || newFiles.Value.Count == 0)
            {
                //TODO newFiles.Value = (await Files.PickFiles()).ToImmutableList();
            }
            //TODO Files.SaveFilesToWalter(Set, target, newFiles.Value.ToList());
            var self = this;
            newFiles.Value.ForEach(f =>
                Liste.Value = Liste.Value.Add(new AnhangListEntry(f, self, Impl)));
            newFiles.Value = newFiles.Value.Clear();
        }
    }

    public sealed class AnhangListEntry
    {
        public Anhang Entity { get; }
        public override string ToString() => Entity.FileName;

        private AnhangListViewModel Container { get; }

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
            // TODO update ViewModel
            if (await Impl.Confirmation())
            {
                Impl.ctx.Anhaenge.Remove(Entity);
                Impl.SaveWalter();
            }
            var deleted = Container.Liste.Value.Find(e => e.Entity.AnhangId == Entity.AnhangId);
            if (deleted != null)
            {
                Container.Liste.Value = Container.Liste.Value.Remove(deleted);
            }
        }

        //public async Task<string> SaveFile()
        public string SaveFile()
        {
            return "";
            //return await Deeplex.Saverwalter.App.Utils.Files.ExtractTo(Entity);
        }

        public string SaveFileTemp()
        {
            var path = Path.Combine(Path.GetTempPath(), Entity.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(Entity.Content, 0, Entity.Content.Length);
            }

            return path;
        }
        public string DateiName => Entity.FileName;

        // TODO
        public string Symbol
        {
            get { return "hi"; }
            //get
            //{
            //    var ext = Path.GetExtension(DateiName);
            //    switch (ext)
            //    {
            //        case "jpg":
            //        case "jpeg":
            //        case "png":
            //            return wuxc.Symbol.Camera.ToString();
            //        default:
            //            return wuxc.Symbol.Document.ToString();
            //    }
            //}
        }
    }
}
