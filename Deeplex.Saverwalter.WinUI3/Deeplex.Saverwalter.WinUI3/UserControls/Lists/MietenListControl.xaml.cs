using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
            App.Window.ListAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            if (((DataGrid)sender).SelectedItem is MietenListViewModelEntry m)
            {
                App.Window.ListAnhang.Value.SetList(m.Entity);
            }
        }
    }
}

