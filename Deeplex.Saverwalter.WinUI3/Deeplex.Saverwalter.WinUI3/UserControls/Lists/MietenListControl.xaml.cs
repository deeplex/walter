using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class MietenListControl : UserControl
    {
        public MietenListControl()
        {
            InitializeComponent();
        }

        public MietenListViewModel ViewModel
        {
            get { return (MietenListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                  "ViewModel",
                  typeof(MietenListViewModel),
                  typeof(VertragListControl),
                  new PropertyMetadata(null));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is MietenListViewModelEntry m)
            {
                App.Window.ListAnhang.Value = new AnhangListViewModel(m.Entity, App.FileService, App.NotificationService, App.WalterService);
            }
            else
            {
                App.Window.ListAnhang.Value = null;
            }
        }
    }
}

