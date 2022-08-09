using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class ZaehlerDetailViewPage : Page
    {
        public ZaehlerDetailViewModel ViewModel { get; set; }

        public ZaehlerDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = App.Container.GetInstance<ZaehlerDetailViewModel>();

            if (e.Parameter is Zaehler zaehler)
            {
                ViewModel.SetEntity(zaehler);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl { ViewModel = ViewModel };
            //App.DetailAnhang.update(ViewModel.Entity, App.WalterService.ctx.ZaehlerAnhaenge);

            base.OnNavigatedTo(e);
        }
    }
}
