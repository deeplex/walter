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
    public sealed partial class ZaehlerListControl : UserControl
    {
        public ZaehlerListControl()
        {
            InitializeComponent();
            ViewModel = App.Container.GetInstance<ZaehlerListViewModel>();
            ViewModel.SetList();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigate.Execute(ViewModel.Selected.Entity);
        }

        public ZaehlerListViewModel ViewModel
        {
            get { return (ZaehlerListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
                = DependencyProperty.Register(
                    "ViewModel",
                    typeof(ZaehlerListViewModel),
                    typeof(ZaehlerListControl),
                    new PropertyMetadata(null));

        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(ZaehlerListControl),
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
            typeof(ZaehlerListControl),
            new PropertyMetadata(0));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Window.ListAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            if (((DataGrid)sender).SelectedItem is ZaehlerListViewModelEntry a)
            {
                App.Window.ListAnhang.Value.SetList(a.Entity);
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.List.Value = (sender as DataGrid).Sort(e.Column, ViewModel.List.Value);
        }
    }
}
