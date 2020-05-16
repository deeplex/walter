using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Deeplex.Saverwalter.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class KalteBetriebskostenRechnungPage : Page
    {
        public KalteBetriebskostenRechnungViewModel ViewModel { get; set; }

        public KalteBetriebskostenRechnungPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int adresseId)
            {
                ViewModel = new KalteBetriebskostenRechnungViewModel(adresseId);
            }
            else if (e.Parameter is null) // New Contact
            {
                // ViewModel = new KalteBetriebskostenRechnungViewModel();
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }

        private void Vorlage_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(KalteBetriebskostenVorlagePage), ViewModel.AdresseId,
                new DrillInNavigationTransitionInfo());
        }
    }
}
