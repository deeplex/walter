using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class MieteListControl : UserControl
    {
        public MieteListControl()
        {
            InitializeComponent();
        }

        public MieteListViewModel ViewModel
        {
            get { return (MieteListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                  "ViewModel",
                  typeof(MieteListViewModel),
                  typeof(VertragListControl),
                  new PropertyMetadata(null));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Window.ListAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            if (((DataGrid)sender).SelectedItem is MieteListViewModelEntry m)
            {
                App.Window.ListAnhang.Value.SetList(m.Entity);
            }
        }
    }
}

