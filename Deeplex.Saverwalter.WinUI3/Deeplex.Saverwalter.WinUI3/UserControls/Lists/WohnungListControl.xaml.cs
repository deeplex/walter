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
        public WohnungListControl()
        {
            InitializeComponent();
            ViewModel = new WohnungListViewModel(App.WalterService, App.NotificationService);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Add.Execute(ViewModel.Selected.Entity);
        }

        public WohnungListViewModel ViewModel
        {
            get { return (WohnungListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                "ViewModel",
                typeof(WohnungListViewModel),
                typeof(WohnungListControl),
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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((WohnungListViewModelEntry)((DataGrid)sender).SelectedItem).Entity;
            App.Window.ListAnhang.Value = new AnhangListViewModel(a, App.FileService, App.NotificationService, App.WalterService);
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.List.Value = (sender as DataGrid).Sort(e.Column, ViewModel.List.Value);
        }
    }
}
