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
            if (e.Parameter is Zaehler zaehler)
            {
                ViewModel = new ZaehlerDetailViewModel(zaehler, App.NotificationService, App.WalterService);
                App.Window.Titel.Value = ViewModel.Kennnummer;
            }
            else if (e.Parameter is ZaehlerDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else if (e.Parameter is null) // New Zaehler
            {
                ViewModel = new ZaehlerDetailViewModel(App.NotificationService, App.WalterService);
            }

            App.Window.CommandBar.MainContent = new ZaehlerCommandBarControl { ViewModel = ViewModel };
            //App.DetailAnhang.update(ViewModel.Entity, App.WalterService.ctx.ZaehlerAnhaenge);

            base.OnNavigatedTo(e);
        }
    }
}
