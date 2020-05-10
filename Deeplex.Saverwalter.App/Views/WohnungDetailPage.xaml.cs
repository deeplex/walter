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
            else // If invoked using "Add"
            {
                //ViewModel = new KontaktViewModel
                //{
                //    IsNewCustomer = true,
                //    IsInEdit = true
                //};
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }

        private void Zaehler_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var a = (Button)sender; // TODO. CommandParameter is null... Like all bindings...
        }
    }
}
