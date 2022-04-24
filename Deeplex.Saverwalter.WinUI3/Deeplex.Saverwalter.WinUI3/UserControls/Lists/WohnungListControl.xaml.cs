using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class WohnungListControl : UserControl
    {
        public WohnungListViewModel ViewModel;

        private void UpdateFilter()
        {
            if (Liste == null)
            {
                ViewModel.Liste.Value = ViewModel.AllRelevant;
            }
            else
            {
                ViewModel.Liste.Value = Liste;
            }

            if (WohnungId != 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    v.Adresse.Wohnungen.Any(w => w.WohnungId == WohnungId))
                    .ToImmutableList();
            }

            if (Filter != "")
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    applyFilter(Filter, v.Bezeichnung, v.Anschrift))
                    .ToImmutableList();
            }
        }

        public WohnungListControl()
        {
            InitializeComponent();
            ViewModel = new WohnungListViewModel(App.WalterService);
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(ListeProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, Prop) => UpdateFilter());
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            App.Window.Navigate(typeof(WohnungDetailViewPage), ViewModel.SelectedWohnung.Value.Entity);
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
                  typeof(WohnungListControl),
                  new PropertyMetadata(0));

        public ImmutableList<WohnungListViewModelEntry> Liste
        {
            get { return (ImmutableList<WohnungListViewModelEntry>)GetValue(ListeProperty); }
            set { SetValue(ListeProperty, value); }
        }

        public static readonly DependencyProperty ListeProperty
            = DependencyProperty.Register(
                  "Liste",
                  typeof(ImmutableList<WohnungListViewModelEntry>),
                  typeof(WohnungListControl),
                  new PropertyMetadata(null));

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty
            = DependencyProperty.Register(
                  "Filter",
                  typeof(string),
                  typeof(WohnungListControl),
                  new PropertyMetadata(""));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((WohnungListViewModelEntry)((DataGrid)sender).SelectedItem).Entity;
            App.Window.ListAnhang.Value = new AnhangListViewModel(a, App.FileService, App.NotificationService, App.WalterService);
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Liste.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Liste.Value);
        }
    }
}
