using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class ErhaltungsaufwendungDetailViewPage : Page
    {
        public ErhaltungsaufwendungDetailViewModel ViewModel { get; } = App.Container.GetInstance<ErhaltungsaufwendungDetailViewModel>();

        public ErhaltungsaufwendungDetailViewPage()
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
