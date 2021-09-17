using Deeplex.Saverwalter.ViewModels.Rechnungen;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{

    public sealed partial class ErhaltungsaufwendungenListViewPage : Page
    {
        public ErhaltungsaufwendungenListViewModel ViewModel = new ErhaltungsaufwendungenListViewModel(App.ViewModel);

        public ErhaltungsaufwendungenListViewPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new ErhaltungsaufwendungenListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
