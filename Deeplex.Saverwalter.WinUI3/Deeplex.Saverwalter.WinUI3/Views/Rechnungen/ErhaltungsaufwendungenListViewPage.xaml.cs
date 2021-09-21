using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

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
