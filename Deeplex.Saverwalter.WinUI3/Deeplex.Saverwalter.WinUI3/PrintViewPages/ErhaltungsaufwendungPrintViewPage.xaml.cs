using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class ErhaltungsaufwendungPrintViewPage : Page
    {
        public ErhaltungsaufwendungPrintViewModel ViewModel { get; } = App.Container.GetInstance<ErhaltungsaufwendungPrintViewModel>();

        public ErhaltungsaufwendungPrintViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
