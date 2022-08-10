using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class VertragListControl : UserControl
    {
        public VertragListControl()
        {
            InitializeComponent();
        }

        public VertragListViewModel ViewModel
        {
            get { return (VertragListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                "ViewModel",
                typeof(VertragListViewModel),
                typeof(VertragListControl),
                new PropertyMetadata(null));

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigate.Execute(ViewModel.Selected.Entity);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Window.ListAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            if ((VertragListViewModelVertrag)((DataGrid)sender).SelectedItem is VertragListViewModelVertrag a)
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
