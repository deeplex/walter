using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class MietMinderungListControl : UserControl
    {
        public MietMinderungListControl()
        {
            InitializeComponent();
        }

        public MietMinderungListViewModel ViewModel
        {
            get { return (MietMinderungListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                  "ViewModel",
                  typeof(MietMinderungListViewModel),
                  typeof(VertragListControl),
                  new PropertyMetadata(null));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Window.ListAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();

            if (((DataGrid)sender).SelectedItem is MietminderungListViewModelEntry m)
            {
                App.Window.ListAnhang.Value.SetList(m.Entity);
            }
        }
    }
}
