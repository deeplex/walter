using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class ZaehlerDetailViewPage : Page
    {
        public ZaehlerDetailViewModel ViewModel { get; set; } = App.Container.GetInstance<ZaehlerDetailViewModel>();

        public ZaehlerDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
