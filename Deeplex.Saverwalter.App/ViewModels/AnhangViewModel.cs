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
using Windows.Storage;
using wuxc = Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AnhangListViewModel : BindableBase
    {
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

        public AnhangListViewModel(Adresse a)
        {
            SetList(a, App.Walter.AdresseAnhaenge);
        }
        public AnhangListViewModel(Betriebskostenrechnung a)
        {
            SetList(a, App.Walter.BetriebskostenrechnungAnhaenge);
        }
        public AnhangListViewModel(Erhaltungsaufwendung a)
        {
            SetList(a, App.Walter.ErhaltungsaufwendungAnhaenge);
        }
        public AnhangListViewModel(Garage a)
        {
            SetList(a, App.Walter.GarageAnhaenge);
        }
        public AnhangListViewModel(JuristischePerson a)
        {
            SetList(a, App.Walter.JuristischePersonAnhaenge);
        }
        public AnhangListViewModel(Konto a)
        {
            SetList(a, App.Walter.KontoAnhaenge);
        }
        public AnhangListViewModel(Miete a)
        {
            SetList(a, App.Walter.MieteAnhaenge);
        }
        public AnhangListViewModel(MietMinderung a)
        {
            SetList(a, App.Walter.MietMinderungAnhaenge);
        }
        public AnhangListViewModel(NatuerlichePerson a)
        {
            SetList(a, App.Walter.NatuerlichePersonAnhaenge);
        }
        public AnhangListViewModel(Vertrag a)
        {
            SetList(a.VertragId, App.Walter.VertragAnhaenge);
        }
        public AnhangListViewModel(Wohnung a)
        {
            SetList(a, App.Walter.WohnungAnhaenge);
        }
        public AnhangListViewModel(Zaehler a)
        {
            SetList(a, App.Walter.ZaehlerAnhaenge);
        }
        public AnhangListViewModel(Zaehlerstand a)
        {
            SetList(a, App.Walter.ZaehlerstandAnhaenge);
        }
    }

    public sealed class AnhangListEntry
    {
        public Anhang Entity { get; }
        public override string ToString() => Entity.FileName;

        public AnhangListEntry(IAnhang a)
        {
            Entity = a.Anhang;

            SaveFile = new AsyncRelayCommand(async _
                => await Files.ExtractTo(Entity), _ => true);

            DeleteFile = new RelayCommand(_ =>
            {
                App.Walter.Anhaenge.Remove(Entity);
                App.SaveWalter();
            }, _ => true);
        }

        public AsyncRelayCommand SaveFile;
        public RelayCommand DeleteFile;
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
