using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class ZaehlerListPage : Page
    {
        public ZaehlerListViewModel ViewModel = new ZaehlerListViewModel(App.ViewModel);

        public ZaehlerListPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new ZaehlerListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
