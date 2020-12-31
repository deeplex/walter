using Deeplex.Saverwalter.App.ViewModels;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }

        public SettingsPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Einstellungen";
            App.ViewModel.RefillCommandContainer();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = new SettingsViewModel();
        }

        private void Adressen_AutoGeneratingColumn(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType != typeof(string))
            {
                e.Cancel = true;
            }
        }

        private void DeleteAdresse_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if ((((Button)sender).Tag is AdresseViewModel vm))
            {
                vm.Dispose.Execute(null);
                ViewModel.Adressen.Value = App.Walter.Adressen.Select(a => new AdresseViewModel(a)).ToImmutableList();
            }
        }
    }
}
