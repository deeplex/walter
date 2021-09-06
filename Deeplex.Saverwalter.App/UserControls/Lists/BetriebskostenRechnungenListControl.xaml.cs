using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Deeplex.Saverwalter.App.Utils.Elements;

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class BetriebskostenRechnungenListControl : UserControl
    {
        public BetriebskostenRechnungenListViewModel ViewModel { get; set; }

        private void UpdateFilter()
        {
            ViewModel.Liste.Value = ViewModel.AllRelevant;
            if (WohnungId != 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value
                    .Where(v => v.Wohnungen.Select(i => i.WohnungId).Contains(WohnungId))
                    .ToImmutableList();
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
                    applyFilter(Filter, v.AdressenBezeichnung, v.BetreffendesJahrString, v.TypString, v.Beschreibung))
                    .ToImmutableList();
            }
        }

        public BetriebskostenRechnungenListControl()
        {
            InitializeComponent();
            ViewModel = new BetriebskostenRechnungenListViewModel();

            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, IdProp) => UpdateFilter());
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedRechnung != null)
            {
                App.ViewModel.Navigate(typeof(BetriebskostenrechnungenDetailPage), ViewModel.SelectedRechnung.Id);
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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender)?.SelectedItem is BetriebskostenRechnungenListEntry r)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(r.Entity));
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Liste.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Liste.Value);
        }
    }
}
