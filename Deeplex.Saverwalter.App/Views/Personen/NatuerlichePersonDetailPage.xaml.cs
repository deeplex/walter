using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
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
                ViewModel = new NatuerlichePersonViewModel(kontakt);
            }
            else if (e.Parameter is null) // New Contact
            {
                ViewModel = new NatuerlichePersonViewModel();
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            App.ViewModel.Titel.Value = ViewModel.Name;

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Attach),
                    Command = ViewModel.AttachFile,
                }
            }, new ICommandBarElement[] { Delete });
            base.OnNavigatedTo(e);
        }

        private void SelfDestruct(object sender, RoutedEventArgs e)
        {
            ViewModel.selfDestruct();
            Frame.GoBack();
        }
    }
}
