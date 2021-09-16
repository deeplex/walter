using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
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
            ViewModel = new SettingsViewModel(App.ViewModel);
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
            if (await App.ViewModel.Confirmation())
            {
                if (((Button)sender).Tag is AdresseViewModel vm)
                {
                    vm.Dispose.Execute(null);
                    ViewModel.Adressen.Value = App.Walter.Adressen.Select(a => new AdresseViewModel(a, App.ViewModel)).ToImmutableList();
                }
            }
        }

        private void DeleteAnhang_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (((Button)sender).Tag is AnhangListEntry vm)
            {
                vm.DeleteFile();
            }
        }

        private void LoadDatabase_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //App.LoadDataBase();
            //ViewModel.LoadAdressen(App.ViewModel);
        }
    }
}
