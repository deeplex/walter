using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class WohnungListViewPage : Page
    {
        public WohnungListViewModel ViewModel = new WohnungListViewModel(App.WalterService);

        public WohnungListViewPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new WohnungListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
