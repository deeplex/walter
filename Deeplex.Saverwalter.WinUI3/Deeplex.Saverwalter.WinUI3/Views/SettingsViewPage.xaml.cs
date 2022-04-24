using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class SettingsViewPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }

        public SettingsViewPage()
        {
            InitializeComponent();

            App.Window.CommandBar.Title = "Einstellungen";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = new SettingsViewModel(App.FileService, App.NotificationService, App.WalterService);
        }

        private void Adressen_AutoGeneratingColumn(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType != typeof(string))
            {
                e.Cancel = true;
            }
        }

        private void Anhaenge_AutoGeneratingColumn(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType != typeof(string))
            {
                e.Cancel = true;
            }
        }

        private async void DeleteAdresse_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (await App.NotificationService.Confirmation())
            {
                if (((Button)sender).Tag is AdresseViewModel vm)
                {
                    vm.Dispose.Execute(null);
                    ViewModel.Adressen.Value = App.WalterService.ctx.Adressen.Select(a => new AdresseViewModel(a, App.WalterService)).ToImmutableList();
                }
            }
        }

        private void DeleteAnhang_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (((Button)sender).Tag is AnhangListViewModelEntry vm)
            {
                vm.DeleteFile();
            }
        }

        private async void LoadDatabase_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.LoadDatabase();
            Utils.Elements.SetDatabaseAsDefault();
        }
    }
}
