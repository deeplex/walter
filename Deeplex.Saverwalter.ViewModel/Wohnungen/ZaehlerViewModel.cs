using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ZaehlerViewModel : BindableBase
    {
        public ZaehlerViewModel self => this;

        private Zaehler Entity;

        public int Id => Entity.ZaehlerId;

        private Zaehler Zaehler { get; }

        public string Kennnummer => Entity.Kennnummer;

        public string Typ => Entity.Typ.ToString();

        public ObservableProperty<ImmutableList<ZaehlerstandViewModel>> Zaehlerstaende
            = new ObservableProperty<ImmutableList<ZaehlerstandViewModel>>();
        public DateTimeOffset AddZaehlerstandDatum => DateTime.UtcNow.Date.AsUtcKind();
        public double AddZaehlerstandStand => Zaehlerstaende.Value.FirstOrDefault()?.Stand ?? 0;
        public void LoadList()
        {
            Zaehlerstaende.Value = Impl.ctx.Zaehlerstaende.ToList()
                .Where(zs => Entity == zs.Zaehler)
                .OrderBy(zs => zs.Datum).Reverse()
                .Select(zs => new ZaehlerstandViewModel(zs, this)).ToImmutableList();
        }

        private IAppImplementation Impl;

        public ZaehlerViewModel(Zaehler z, IAppImplementation impl)
        {
            Zaehler = z;
            Impl = impl;

            LoadList();

            AddZaehlerstand = new RelayCommand(AddZaehlerstandPanel
                => mAddZaehlerstand(AddZaehlerstandPanel, z), _ => true);

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.ZaehlerAnhaenge, z), _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand AddZaehlerstand { get; }

        private void mAddZaehlerstand(object AddZaehlerstandPanel, Zaehler z)
        {
            // TODO 
            //var dtp = ((CalendarDatePicker)((StackPanel)AddZaehlerstandPanel).Children[0]).Date;
            //var datum = (dtp.HasValue ? dtp.Value.UtcDateTime : DateTime.UtcNow.Date).AsUtcKind();
            //var stand = Convert.ToDouble(((NumberBox)((StackPanel)AddZaehlerstandPanel).Children[1]).Text);

            //var zs = new Zaehlerstand
            //{
            //    AllgemeinZaehler = az,
            //    Zaehler = z,
            //    Datum = datum,
            //    Stand = stand,
            //};
            //App.Walter.Zaehlerstaende.Add(zs);
            //App.SaveWalter();
            //var wdzs = new ZaehlerstandViewModel(zs, this);
            //Zaehlerstaende.Value = Zaehlerstaende.Value
            //    .Add(wdzs)
            //    .OrderBy(nzs => nzs.Datum).Reverse()
            //    .ToImmutableList();
            //RaisePropertyChanged(nameof(Zaehlerstaende));
        }
    }
}
