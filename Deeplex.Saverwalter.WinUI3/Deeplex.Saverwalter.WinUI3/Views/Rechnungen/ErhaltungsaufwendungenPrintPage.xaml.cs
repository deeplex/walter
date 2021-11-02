using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{
    public sealed partial class ErhaltungsaufwendungenPrintPage : Page
    {
        public ErhaltungsaufwendungenPrintViewModel ViewModel { get; set; }

        public ErhaltungsaufwendungenPrintPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Wohnung w)
            {
                ViewModel = new ErhaltungsaufwendungenPrintViewModel(w, App.ViewModel, App.Impl);
            }
            else if (e.Parameter is IPerson p)
            {
                ViewModel = new ErhaltungsaufwendungenPrintViewModel(p, App.ViewModel, App.Impl);
            }

            App.Window.CommandBar.MainContent = new ErhaltungsaufwendungenPrintCommandBarControl { ViewModel = ViewModel };

            base.OnNavigatedTo(e);
        }
    }
}
