using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class KontaktListControl : UserControl
    {
        public KontaktListControl()
        {
            InitializeComponent();
        }

        public KontaktListViewModel ViewModel
        {
            get { return (KontaktListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                "ViewModel",
                typeof(KontaktListViewModel),
                typeof(VertragListControl),
                new PropertyMetadata(null));

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigate.Execute(ViewModel.Selected.Entity);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Window.ListAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            var a = ((KontaktListViewModelEntry)((DataGrid)sender).SelectedItem)?.Entity;
            if (a is NatuerlichePerson n)
            {
                App.Window.ListAnhang.Value.SetList(n);
            }
            else if (a is JuristischePerson j)
            {
                App.Window.ListAnhang.Value.SetList(j);
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.List.Value = (sender as DataGrid).Sort(e.Column, ViewModel.List.Value);
        }
    }
}
