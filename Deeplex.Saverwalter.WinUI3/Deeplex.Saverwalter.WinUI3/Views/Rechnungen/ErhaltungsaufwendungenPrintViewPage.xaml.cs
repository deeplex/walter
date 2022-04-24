using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{
    public sealed partial class ErhaltungsaufwendungenPrintViewPage : Page
    {
        public ErhaltungsaufwendungenPrintViewModel ViewModel { get; set; }

        public ErhaltungsaufwendungenPrintViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Wohnung w)
            {
                ViewModel = new ErhaltungsaufwendungenPrintViewModel(w, App.WalterService, App.FileService);
            }
            else if (e.Parameter is IPerson p)
            {
                ViewModel = new ErhaltungsaufwendungenPrintViewModel(p, App.WalterService, App.FileService);
            }

            App.Window.CommandBar.MainContent = new ErhaltungsaufwendungenPrintCommandBarControl { ViewModel = ViewModel };

            base.OnNavigatedTo(e);
        }
    }
}
