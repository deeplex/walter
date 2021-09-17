using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class ZaehlerDetailPage : Page
    {
        public ZaehlerDetailViewModel ViewModel { get; set; }

        public ZaehlerDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Zaehler zaehler)
            {
                ViewModel = new ZaehlerDetailViewModel(zaehler, App.Impl, App.ViewModel);
                App.ViewModel.Titel.Value = ViewModel.Kennnummer;
            }
            else if (e.Parameter is ZaehlerDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else if (e.Parameter is null) // New Zaehler
            {
                ViewModel = new ZaehlerDetailViewModel(App.Impl, App.ViewModel);
            }

            App.Window.CommandBar.MainContent = new ZaehlerCommandBarControl { ViewModel = ViewModel };
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }
    }
}
