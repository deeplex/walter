using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class ZaehlerViewModel : BindableBase
    {
        public ZaehlerViewModel self => this;

        private object Entity
        {
            get
            {
                if (Zaehler == null)
                {
                    return AllgemeinZaehler;
                }
                else
                {
                    return Zaehler;
                }
            }
        }

        public int Id
        {
            get => Entity is Zaehler e ? e.ZaehlerId :
                Entity is AllgemeinZaehler a ? a.AllgemeinZaehlerId : 0;
        }

        private Zaehler Zaehler { get; }
        private AllgemeinZaehler AllgemeinZaehler { get; }

        public bool isAllgemein => AllgemeinZaehler != null;
        public string AllgemeinString => isAllgemein ? " Allgemein" : "";

        public string Kennnummer
        {
            get => Entity is Zaehler e ? e.Kennnummer :
                Entity is AllgemeinZaehler a ? a.Kennnummer : "";
        }

        public string Typ
        {
            get => Entity is Zaehler e ? e.Typ.ToString() :
                Entity is AllgemeinZaehler a ? a.Typ.ToString() : "";
        }

        public ObservableProperty<ImmutableList<ZaehlerstandViewModel>> Zaehlerstaende
            = new ObservableProperty<ImmutableList<ZaehlerstandViewModel>>();
        public DateTimeOffset AddZaehlerstandDatum => DateTime.UtcNow.Date.AsUtcKind();
        public double AddZaehlerstandStand => Zaehlerstaende.Value.FirstOrDefault()?.Stand ?? 0;
        public void LoadList()
        {
            Zaehlerstaende.Value = App.Walter.Zaehlerstaende.ToList()
                .Where(zs => Entity == zs.Zaehler || Entity == zs.AllgemeinZaehler)
                .OrderBy(zs => zs.Datum).Reverse()
                .Select(zs => new ZaehlerstandViewModel(zs)).ToImmutableList();
        }

        public ZaehlerViewModel(Zaehler z)
        {
            Zaehler = z;
            
            LoadList();

            AddZaehlerstand = new RelayCommand(AddZaehlerstandPanel
                => mAddZaehlerstand(AddZaehlerstandPanel, z, null), _ => true);

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.ZaehlerAnhaenge, z), _ => true);
        }

        public ZaehlerViewModel(AllgemeinZaehler z)
        {
            AllgemeinZaehler = z;

            LoadList();

            AddZaehlerstand = new RelayCommand(AddZaehlerstandPanel
                => mAddZaehlerstand(AddZaehlerstandPanel, null, z), _ => true);


            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.AllgemeinZaehlerAnhaenge, z), _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand AddZaehlerstand { get; }

        private void mAddZaehlerstand(object AddZaehlerstandPanel, Zaehler z, AllgemeinZaehler az)
        {
            var dtp = ((CalendarDatePicker)((StackPanel)AddZaehlerstandPanel).Children[0]).Date;
            var datum = (dtp.HasValue ? dtp.Value.UtcDateTime : DateTime.UtcNow.Date).AsUtcKind();
            var stand = Convert.ToDouble(((NumberBox)((StackPanel)AddZaehlerstandPanel).Children[1]).Text);

            var zs = new Zaehlerstand
            {
                AllgemeinZaehler = az,
                Zaehler = z,
                Datum = datum,
                Stand = stand,
            };
            App.Walter.Zaehlerstaende.Add(zs);
            App.Walter.SaveChanges();
            var wdzs = new ZaehlerstandViewModel(zs);
            Zaehlerstaende.Value = Zaehlerstaende.Value
                .Add(wdzs)
                .OrderBy(nzs => nzs.Datum).Reverse()
                .ToImmutableList();
            RaisePropertyChanged(nameof(Zaehlerstaende));
        }
    }
}
