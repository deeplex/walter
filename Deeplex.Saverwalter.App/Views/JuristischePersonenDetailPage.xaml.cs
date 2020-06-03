using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class JuristischePersonenDetailPage : Page
    {
        public JuristischePersonViewModel ViewModel { get; set; }

        public JuristischePersonenDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int jpId)
            {
                ViewModel = new JuristischePersonViewModel(jpId);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new JuristischePersonViewModel();
            }

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
