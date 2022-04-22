using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class MietenListControl : UserControl
    {
        public MietenListViewModel ViewModel { get; set; }

        public MietenListControl()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(VertragGuidProperty, (DepObj, Prop) =>
            {
                ViewModel = new MietenListViewModel(VertragGuid, App.Impl, App.ViewModel);
            });
        }

        public Guid VertragGuid
        {
            get { return (Guid)GetValue(VertragGuidProperty); }
            set { SetValue(VertragGuidProperty, value); }
        }

        public static readonly DependencyProperty VertragGuidProperty
            = DependencyProperty.Register(
                  "VertragGuid",
                  typeof(Guid),
                  typeof(VertragListControl),
                  new PropertyMetadata(Guid.Empty));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is MietenListViewModelEntry m)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(m.Entity, App.Impl, App.ViewModel));
            }
            else
            {
                App.ViewModel.clearAnhang();
            }
        }
    }
}

