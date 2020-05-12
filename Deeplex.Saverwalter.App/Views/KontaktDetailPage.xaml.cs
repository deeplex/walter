using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class KontaktDetailPage : Page
    {
        public KontaktDetailViewModel ViewModel { get; set; }

        public KontaktDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int kontaktId)
            {
                ViewModel = new KontaktDetailViewModel(kontaktId);
            }
            else if (e.Parameter is null) // New Contact
            {
                ViewModel = new KontaktDetailViewModel();
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }
    }
}
