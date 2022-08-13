using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class ErhaltungsaufwendungenDetailViewPage : Page
    {
        public ErhaltungsaufwendungDetailViewModel ViewModel { get; } = App.Container.GetInstance<ErhaltungsaufwendungDetailViewModel>();

        public ErhaltungsaufwendungenDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Erhaltungsaufwendung r)
            {
                ViewModel.SetEntity(r);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl { ViewModel = ViewModel }; // TODO123

            base.OnNavigatedTo(e);
        }
    }
}
