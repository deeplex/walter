using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class NatuerlichePersonDetailPage : Page
    {
        public NatuerlichePersonViewModel ViewModel { get; set; }

        public NatuerlichePersonDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NatuerlichePerson kontakt)
            {
                ViewModel = new NatuerlichePersonViewModel(kontakt, App.ViewModel);
            }
            else if (e.Parameter is null) // New Contact
            {
                ViewModel = new NatuerlichePersonViewModel(App.ViewModel);
            }

            App.ViewModel.Titel.Value = ViewModel.Name;
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.ViewModel));

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.ViewModel.RefillCommandContainer(
                new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });
            base.OnNavigatedTo(e);
        }

        private async void SelfDestruct(object sender, RoutedEventArgs e)
        {
            if (await App.ViewModel.Confirmation())
            {
                ViewModel.selfDestruct();
                Frame.GoBack();
            }
        }
    }
}
