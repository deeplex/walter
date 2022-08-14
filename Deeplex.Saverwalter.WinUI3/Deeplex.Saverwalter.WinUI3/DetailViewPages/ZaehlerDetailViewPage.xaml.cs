using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class ZaehlerDetailViewPage : Page
    {
        public ZaehlerDetailViewModel ViewModel { get; } = App.Container.GetInstance<ZaehlerDetailViewModel>();
        public ZaehlerListViewModel ZaehlerViewModel { get; } = App.Container.GetInstance<ZaehlerListViewModel>();

        public ZaehlerDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Zaehler zaehler)
            {
                ViewModel.SetEntity(zaehler);
                ZaehlerViewModel.SetList(zaehler);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl { ViewModel = ViewModel };
            //App.DetailAnhang.update(ViewModel.Entity, App.WalterService.ctx.ZaehlerAnhaenge);

            base.OnNavigatedTo(e);
        }
    }
}
