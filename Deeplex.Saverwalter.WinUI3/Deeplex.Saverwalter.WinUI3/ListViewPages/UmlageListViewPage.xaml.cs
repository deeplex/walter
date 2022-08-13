using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class UmlageListViewPage : Page
    {
        public UmlageListViewModel ViewModel { get; } = App.Container.GetInstance<UmlageListViewModel>();

        public UmlageListViewPage()
        {
            InitializeComponent();
            ViewModel.SetList();
            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
