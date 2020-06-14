using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System.Linq;
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
            BesitzerCombobox.SelectedIndex = ViewModel.AlleVermieter.FindIndex(v => v.Id == ViewModel.Besitzer.Id);
            base.OnNavigatedTo(e);
        }

        private void UpdateAdresse_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var adress = App.Walter.Adressen.FirstOrDefault(a =>
                a.Strasse == ComboBoxStrasse.Text &&
                a.Hausnummer == ComboBoxHausnummer.Text &&
                a.Postleitzahl == ComboBoxPostleitzahl.Text &&
                a.Stadt == ComboBoxStadt.Text);
            if (adress != null)
            {
                ViewModel.Adresse = new AdresseViewModel(adress);
            }
            else
            {
                var a = new Adresse
                {
                    Strasse = ComboBoxStrasse.Text,
                    Hausnummer = ComboBoxHausnummer.Text,
                    Postleitzahl = ComboBoxPostleitzahl.Text,
                    Stadt = ComboBoxStadt.Text,
                };
                App.Walter.Adressen.Add(a);
                ViewModel.Adresse = new AdresseViewModel(a);
            }
        }

        private void EditToggle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsInEdit.Value = EditToggle.IsChecked ?? false;
        }
    }
}
