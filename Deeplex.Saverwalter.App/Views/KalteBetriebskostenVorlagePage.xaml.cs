using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class KalteBetriebskostenVorlagePage : Page
    {
        public KalteBetriebskostenVorlageViewModel ViewModel { get; set; }

        public KalteBetriebskostenVorlagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int adresseId)
            {
                ViewModel = new KalteBetriebskostenVorlageViewModel(adresseId);
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }
    }
}
