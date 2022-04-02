using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class BetriebskostenRechnungenListControl : UserControl
    {
        public BetriebskostenRechnungenListViewModel ViewModel { get; set; }
        public ObservableProperty<ImmutableList<BetriebskostenRechnungenListEntry>> Templates =
            new ObservableProperty<ImmutableList<BetriebskostenRechnungenListEntry>>();

        private void UpdateFilter()
        {
            ViewModel.Liste.Value = ViewModel.AllRelevant;
            if (WohnungId != 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value
                    .Where(v => v.Wohnungen.Select(i => i.WohnungId).Contains(WohnungId))
                    .ToImmutableList();

                if (ZeigeVorlagen)
                {
                    Templates.Value = ViewModel.Liste.Value.Where(w => !ViewModel.Liste.Value.Exists(r =>
                        r.Typ == w.Typ && r.BetreffendesJahr == w.BetreffendesJahr + 1))
                            .Select(e => new BetriebskostenRechnungenListEntry(e.Entity.NewYear(), true))
                            .ToImmutableList();

                ViewModel.Liste.Value = ViewModel.Liste.Value.Concat(Templates.Value).ToImmutableList();
                }
            }
            if (StartJahr != 0 && EndeJahr != 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value
                    .Where(v => v.BetreffendesJahr >= StartJahr && v.BetreffendesJahr <= EndeJahr)
                    .ToImmutableList();
            }
            if (Filter != "")
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    applyFilter(Filter, v.AdressenBezeichnung, v.BetreffendesJahr.ToString("dd.mm.yyyy"), v.Typ.ToDescriptionString(), v.Beschreibung))
                    .ToImmutableList();
            }

            if (BetreffendesJahr > 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v => v.BetreffendesJahr == BetreffendesJahr).ToImmutableList();
            }
        }

        public BetriebskostenRechnungenListControl()
        {
            InitializeComponent();
            ViewModel = new BetriebskostenRechnungenListViewModel(App.ViewModel);

            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(ZeigeVorlagenProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(BetreffendesJahrProperty, (DepObj, IdProp) => UpdateFilter());
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedRechnung != null)
            {
                if (WohnungId != 0)
                {
                    App.Window.Navigate(typeof(BetriebskostenrechnungenDetailPage), new Tuple<Betriebskostenrechnung, int>(ViewModel.SelectedRechnung.Entity, WohnungId));
                }
                else
                {
                    App.Window.Navigate(typeof(BetriebskostenrechnungenDetailPage), ViewModel.SelectedRechnung.Entity);
                }
            }
        }

        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));

        public int StartJahr
        {
            get { return (int)GetValue(StartJahrProperty); }
            set { SetValue(StartJahrProperty, value); }
        }

        public static readonly DependencyProperty StartJahrProperty
            = DependencyProperty.Register(
                  "StartJahr",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));

        public int EndeJahr
        {
            get { return (int)GetValue(EndeJahrProperty); }
            set { SetValue(EndeJahrProperty, value); }
        }

        public static readonly DependencyProperty EndeJahrProperty
            = DependencyProperty.Register(
                  "EndeJahr",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty
            = DependencyProperty.Register(
                  "Filter",
                  typeof(string),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(""));

        public int BetreffendesJahr
        {
            get { return (int)GetValue(BetreffendesJahrProperty); }
            set { SetValue(BetreffendesJahrProperty, value); }
        }

        public static readonly DependencyProperty BetreffendesJahrProperty
            = DependencyProperty.Register(
                  "BetreffendesJahr",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));

        public bool ZeigeVorlagen
        {
            get { return (bool)GetValue(ZeigeVorlagenProperty); }
            set { SetValue(ZeigeVorlagenProperty, value); }
        }

        public static readonly DependencyProperty ZeigeVorlagenProperty
            = DependencyProperty.Register(
                "ZeigeVorlagen",
                typeof(bool),
                typeof(BetriebskostenRechnungenListControl),
                new PropertyMetadata(false));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender)?.SelectedItem is BetriebskostenRechnungenListEntry r)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(r.Entity, App.Impl, App.ViewModel));
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Liste.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Liste.Value);
        }
    }
}
