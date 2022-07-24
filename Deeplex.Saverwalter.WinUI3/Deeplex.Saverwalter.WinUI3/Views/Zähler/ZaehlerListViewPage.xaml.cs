using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class ZaehlerListViewPage : Page
    {
        public ZaehlerListViewModel ViewModel { get; } = new ZaehlerListViewModel(App.WalterService);

        public ZaehlerListViewPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
