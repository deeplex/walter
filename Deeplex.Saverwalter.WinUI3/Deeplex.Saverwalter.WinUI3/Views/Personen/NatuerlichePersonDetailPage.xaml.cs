using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
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
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.Impl, App.ViewModel));

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.Window.RefillCommandContainer(
                new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });
            base.OnNavigatedTo(e);
        }

        private async void SelfDestruct(object sender, RoutedEventArgs e)
        {
            if (await App.Impl.Confirmation())
            {
                ViewModel.selfDestruct();
                Frame.GoBack();
            }
        }
    }
}
