using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3
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
            ViewModel = App.Container.GetInstance<SettingsViewModel>();
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
            var db = App.Container.GetInstance<IWalterDbService>();
            var ns = App.Container.GetInstance<INotificationService>();
            if (await ns.Confirmation())
            {
                if (((Button)sender).Tag is AdresseViewModel vm)
                {
                    vm.Delete.Execute(null);
                    ViewModel.Adressen.Value = db.ctx.Adressen.Select(a => new AdresseViewModel(a, db, ns)).ToImmutableList();
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

        private void LoadDatabase_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // TODO
            //SetDatabaseAsDefault(await ViewModel.LoadDatabase());
        }
    }
}
