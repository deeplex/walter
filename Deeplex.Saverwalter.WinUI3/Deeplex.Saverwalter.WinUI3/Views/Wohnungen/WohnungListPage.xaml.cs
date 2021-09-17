using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class WohnungListPage : Page
    {
        public WohnungListViewModel ViewModel = new WohnungListViewModel(App.ViewModel);

        public WohnungListPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new WohnungListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
