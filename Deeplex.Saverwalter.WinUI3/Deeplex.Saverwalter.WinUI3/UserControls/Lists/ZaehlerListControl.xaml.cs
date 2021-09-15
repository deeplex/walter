using Deeplex.Saverwalter.WinUI3.Views;
using Deeplex.Saverwalter.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ZaehlerListControl : UserControl
    {
        public ZaehlerListViewModel ViewModel { get; set; }

        private void UpdateFilter()
        {
            ViewModel.Liste.Value = ViewModel.AllRelevant;
            if (WohnungId != 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    v.WohnungId == WohnungId).ToImmutableList();
            }
            if (ZaehlerId != 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value
                    .Where(v => v.AllgemeinZaehler?.ZaehlerId == ZaehlerId)
                    .ToImmutableList();
            }
            if (Filter != "")
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    applyFilter(Filter, v.Kennnummer, v.Wohnung, v.TypString))
                    .ToImmutableList();
            }
        }

        public ZaehlerListControl()
        {
            InitializeComponent();
            ViewModel = new ZaehlerListViewModel(App.ViewModel);

            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(ZaehlerIdProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty
            = DependencyProperty.Register(
                  "Filter",
                  typeof(string),
                  typeof(VertragListControl),
                  new PropertyMetadata(""));

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedZaehler != null)
            {
                App.ViewModel.Navigate(
                    typeof(ZaehlerDetailPage),
                    App.Walter.ZaehlerSet.Find(ViewModel.SelectedZaehler.Id));
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
                  typeof(VertragListControl),
                  new PropertyMetadata(0));

        public int ZaehlerId
        {
            get { return (int)GetValue(ZaehlerIdProperty); }
            set { SetValue(ZaehlerIdProperty, value); }
        }

        public static readonly DependencyProperty ZaehlerIdProperty
            = DependencyProperty.Register(
            "ZaehlerId",
            typeof(int),
            typeof(ZaehlerstandListControl),
            new PropertyMetadata(0));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((ZaehlerListEntry)((DataGrid)sender).SelectedItem).Entity;
            App.ViewModel.updateListAnhang(new AnhangListViewModel(a, App.ViewModel));
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Liste.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Liste.Value);
        }
    }
}
