using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class ZaehlerListViewPage : Page
    {
        public ZaehlerListViewModel ViewModel { get; } = App.Container.GetInstance<ZaehlerListViewModel>();

        public ZaehlerListViewPage()
        {
            InitializeComponent();
            ViewModel.SetList();
            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
