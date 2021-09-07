using Deeplex.Saverwalter.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class MietMinderungListControl : UserControl
    {
        public MietMinderungListViewModel ViewModel { get; set; }

        public MietMinderungListControl()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(VertragGuidProperty, (DepObj, Prop) =>
            {
                ViewModel = new MietMinderungListViewModel(VertragGuid, App.ViewModel);
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
            if (((DataGrid)sender).SelectedItem is MietMinderungListEntry m)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(m.Entity, App.ViewModel));
            }
            else
            {
                App.ViewModel.clearAnhang();
            }
        }
    }
}
