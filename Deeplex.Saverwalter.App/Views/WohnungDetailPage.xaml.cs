using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class WohnungDetailPage : Page
    {
        public WohnungDetailViewModel ViewModel { get; set; }

        public WohnungDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int wohnungId)
            {
                ViewModel = new WohnungDetailViewModel(wohnungId);
            }
            else if (e.Parameter is null) // New Wohnung
            {
                ViewModel = new WohnungDetailViewModel();
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }
    }
}
