using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
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
            if (await App.ViewModel.Confirmation())
            {
                ViewModel.SelfDestruct();
                Frame.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Zaehler zaehler)
            {
                ViewModel = new ZaehlerDetailViewModel(zaehler, App.ViewModel);
                App.ViewModel.Titel.Value = ViewModel.Kennnummer;
            }
            else if (e.Parameter is ZaehlerDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else if (e.Parameter is null) // New Zaehler
            {
                ViewModel = new ZaehlerDetailViewModel(App.ViewModel);
            }

            App.ViewModel.Titel.Value = ViewModel == null ? "Neuer Zähler" : ViewModel.Kennnummer;
            var Delete = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = "Löschen",
            };
            Delete.Click += Delete_Click;
            App.ViewModel.RefillCommandContainer(new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.ViewModel));

            base.OnNavigatedTo(e);
        }
    }
}
