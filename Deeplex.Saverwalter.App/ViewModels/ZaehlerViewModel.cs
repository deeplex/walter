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
        public int Id;
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

        private Zaehler Zaehler;
        private AllgemeinZaehler AllgemeinZaehler;

        public string AllgemeinString => AllgemeinZaehler != null ? " Allgemein" : "";

        public ObservableProperty<string> Kennnummer = new ObservableProperty<string>();
        public ObservableProperty<string> Typ = new ObservableProperty<string>();
        public ObservableProperty<ImmutableList<ZaehlerstandViewModel>> Zaehlerstaende
            = new ObservableProperty<ImmutableList<ZaehlerstandViewModel>>();
        public DateTimeOffset AddZaehlerstandDatum => DateTime.UtcNow.Date.AsUtcKind();
        // TODO interpolate between last and prelast to determine stand
        public double AddZaehlerstandStand => Zaehlerstaende.Value.FirstOrDefault()?.Stand ?? 0;
        public void LoadList()
        {
            Zaehlerstaende.Value = App.Walter.Zaehlerstaende
                .Where(zs => zs.Zaehler == Entity)
                .Select(zs => new ZaehlerstandViewModel(zs))
                .ToList()
                .OrderBy(zs => zs.Datum).Reverse()
                .ToImmutableList();
        }

        public ZaehlerViewModel(Zaehler z)
        {
            Id = z.ZaehlerId;
            Zaehler = z;
            Kennnummer.Value = z.Kennnummer;
            Typ.Value = z.Typ.ToString(); // May be a descript thingy later on?...

            Zaehlerstaende.Value = App.Walter.Zaehlerstaende
                .Where(zs => zs.Zaehler == Entity)
                .Select(zs => new ZaehlerstandViewModel(zs))
                .ToList()
                .OrderBy(zs => zs.Datum).Reverse()
                .ToImmutableList();

            AddZaehlerstand = new RelayCommand(AddZaehlerstandPanel =>
            {
                var dtp = ((CalendarDatePicker)((StackPanel)AddZaehlerstandPanel).Children[0]).Date;
                var datum = (dtp.HasValue ? dtp.Value.UtcDateTime : DateTime.UtcNow.Date).AsUtcKind();
                var stand = Convert.ToDouble(((NumberBox)((StackPanel)AddZaehlerstandPanel).Children[1]).Text);

                var zs = new Zaehlerstand
                {
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
            }, _ => true);


            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.ZaehlerAnhaenge, z), _ => true);
        }

        public ZaehlerViewModel(AllgemeinZaehler z)
        {
            Id = z.AllgemeinZaehlerId;
            AllgemeinZaehler = z;
            Kennnummer.Value = z.Kennnummer;
            Typ.Value = z.Typ.ToString(); // May be a descript thingy later on?...

            Zaehlerstaende.Value = App.Walter.Zaehlerstaende
                .Where(zs => zs.AllgemeinZaehler == Entity)
                .Select(zs => new ZaehlerstandViewModel(zs))
                .ToList()
                .OrderBy(zs => zs.Datum).Reverse()
                .ToImmutableList();

            AddZaehlerstand = new RelayCommand(AddZaehlerstandPanel =>
            {
                var dtp = ((CalendarDatePicker)((StackPanel)AddZaehlerstandPanel).Children[0]).Date;
                var datum = (dtp.HasValue ? dtp.Value.UtcDateTime : DateTime.UtcNow.Date).AsUtcKind();
                var stand = Convert.ToDouble(((NumberBox)((StackPanel)AddZaehlerstandPanel).Children[1]).Text);

                var zs = new Zaehlerstand
                {
                    AllgemeinZaehler = z,
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
            }, _ => true);


            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.AllgemeinZaehlerAnhaenge, z), _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand AddZaehlerstand { get; }
    }
}
