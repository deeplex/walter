using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

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
