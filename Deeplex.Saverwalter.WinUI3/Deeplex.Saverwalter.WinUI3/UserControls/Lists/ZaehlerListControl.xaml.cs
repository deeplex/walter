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
            ViewModel = new ZaehlerListViewModel(App.WalterService);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Selected != null)
            {
                App.Window.Navigate(
                    typeof(ZaehlerDetailViewPage),
                    App.WalterService.ctx.ZaehlerSet.Find(ViewModel.Selected.Id));
            }
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
            var a = ((ZaehlerListViewModelEntry)((DataGrid)sender).SelectedItem).Entity;
            App.Window.ListAnhang.Value = new AnhangListViewModel(a, App.FileService, App.NotificationService, App.WalterService);
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.List.Value = (sender as DataGrid).Sort(e.Column, ViewModel.List.Value);
        }
    }
}
