using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class WohnungListControl : UserControl
    {
        public WohnungListControl()
        {
            InitializeComponent();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigate.Execute(ViewModel.Selected.Entity);
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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Window.ListAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            if (((DataGrid)sender).SelectedItem is WohnungListViewModelEntry a)
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
