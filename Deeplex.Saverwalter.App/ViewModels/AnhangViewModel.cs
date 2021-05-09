using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using wuxc = Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AnhangListViewModel : BindableBase
    {
        public ObservableProperty<string> Text = new ObservableProperty<string>();
        public ObservableProperty<ImmutableList<AnhangListEntry>> Liste =
            new ObservableProperty<ImmutableList<AnhangListEntry>>();

        private void SetList<T>(T a, IQueryable<IAnhang<T>> set)
        {
            Liste.Value = set.Include(e => e.Anhang)
                .ToList()
                .Where(b => Equals(b.Target, a))
                .ToList()
                .Select(e => new AnhangListEntry(e))
                .ToImmutableList();
        }

        public AsyncRelayCommand AddAnhang;

        public AnhangListViewModel(Adresse a)
        {
            Text.Value = AdresseViewModel.Anschrift(a);
            SetList(a, App.Walter.AdresseAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.AdresseAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Betriebskostenrechnung a)
        {
            Text.Value = a.BetreffendesJahr.ToString() + ", " + a.Schluessel.ToDescriptionString();
            SetList(a, App.Walter.BetriebskostenrechnungAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.BetriebskostenrechnungAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Erhaltungsaufwendung a)
        {
            Text.Value = a.Bezeichnung;
            SetList(a, App.Walter.ErhaltungsaufwendungAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.ErhaltungsaufwendungAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Garage a)
        {
            Text.Value = AdresseViewModel.Anschrift(a.Adresse) + ", " + a.Kennung;
            SetList(a, App.Walter.GarageAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.GarageAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(JuristischePerson a)
        {
            Text.Value = a.Bezeichnung;
            SetList(a, App.Walter.JuristischePersonAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.JuristischePersonAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Konto a)
        {
            Text.Value = a.Iban;
            SetList(a, App.Walter.KontoAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.KontoAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Miete a)
        {
            Text.Value = "Miete: " + a.BetreffenderMonat.ToString("mm.yyyy");
            SetList(a, App.Walter.MieteAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.MieteAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(MietMinderung a)
        {
            Text.Value = "Mietminderung";
            SetList(a, App.Walter.MietMinderungAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.MietMinderungAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(NatuerlichePerson a)
        {
            Text.Value = a.Bezeichnung;
            SetList(a, App.Walter.NatuerlichePersonAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.NatuerlichePersonAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Vertrag a)
        {
            Text.Value = "Vertrag: " + AdresseViewModel.Anschrift(a.Wohnung) + ", " + a.Wohnung.Bezeichnung;
            SetList(a.VertragId, App.Walter.VertragAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.VertragAnhaenge, a.VertragId),
                _ => true);
        }
        public AnhangListViewModel(Wohnung a)
        {
            Text.Value = AdresseViewModel.Anschrift(a) + ", " + a.Bezeichnung;
            SetList(a, App.Walter.WohnungAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.WohnungAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Zaehler a)
        {
            Text.Value = a.Kennnummer;
            SetList(a, App.Walter.ZaehlerAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.ZaehlerAnhaenge, a),
                _ => true);
        }
        public AnhangListViewModel(Zaehlerstand a)
        {
            Text.Value = a.Datum.ToString("dd.MM.yyyy") + ": " + a.Stand.ToString();
            SetList(a, App.Walter.ZaehlerstandAnhaenge);
            AddAnhang = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.ZaehlerstandAnhaenge, a),
                _ => true);
        }
    }

    public sealed class AnhangListEntry
    {
        public Anhang Entity { get; }
        public override string ToString() => Entity.FileName;

        public AnhangListEntry(IAnhang a)
        {
            Entity = a.Anhang;
        }

        public async void DeleteFile()
        {
            // TODO update ViewModel
            if (await App.ViewModel.Confirmation())
            {
                App.Walter.Anhaenge.Remove(Entity);
                App.SaveWalter();
            }
        }

        public async Task<string> SaveFile()
        {
            return await Files.ExtractTo(Entity);
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
        public string Symbol
        {
            get
            {
                var ext = Path.GetExtension(DateiName);
                switch (ext)
                {
                    case "jpg":
                    case "jpeg":
                    case "png":
                        return wuxc.Symbol.Camera.ToString();
                    default:
                        return wuxc.Symbol.Document.ToString();
                }
            }
        }
    }
}
