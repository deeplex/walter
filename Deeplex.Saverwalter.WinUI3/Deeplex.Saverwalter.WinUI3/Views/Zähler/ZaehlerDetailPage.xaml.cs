using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
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

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SelfDestruct();
            Frame.GoBack();
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

            App.ViewModel.Titel.Value = ViewModel == null ? "Neuer Zähler" : ViewModel.Kennnummer;
            var Delete = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = "Löschen",
            };
            Delete.Click += Delete_Click;
            App.Window.RefillCommandContainer(new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }
    }
}
