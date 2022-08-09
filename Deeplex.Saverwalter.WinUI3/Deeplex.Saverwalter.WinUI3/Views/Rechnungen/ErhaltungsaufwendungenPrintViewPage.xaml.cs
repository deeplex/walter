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
            ViewModel = App.Container.GetInstance<ErhaltungsaufwendungenPrintViewModel>();
            if (e.Parameter is Wohnung w)
            {
                ViewModel.SetEntity(w);
            }
            else if (e.Parameter is IPerson p)
            {
                ViewModel.SetEntity(p);
            }

            App.Window.CommandBar.MainContent = new PrintCommandBarControl { ViewModel = ViewModel };

            base.OnNavigatedTo(e);
        }
    }
}
