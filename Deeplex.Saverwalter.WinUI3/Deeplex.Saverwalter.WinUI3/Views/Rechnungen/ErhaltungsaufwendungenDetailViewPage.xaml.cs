using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class ErhaltungsaufwendungenDetailViewPage : Page
    {
        public ErhaltungsaufwendungenDetailViewModel ViewModel { get; set; }

        public ErhaltungsaufwendungenDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = App.Container.GetInstance<ErhaltungsaufwendungenDetailViewModel>();
            if (e.Parameter is Erhaltungsaufwendung r)
            {
                ViewModel.SetEntity(r);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl { ViewModel = ViewModel }; // TODO123

            base.OnNavigatedTo(e);
        }
    }
}
