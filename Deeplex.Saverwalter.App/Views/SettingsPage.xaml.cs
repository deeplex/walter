using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.App.ViewModels;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Windows.Storage;
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

        private async void DeleteAdresse_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (await App.ViewModel.Confirmation())
            {
                if (((Button)sender).Tag is AdresseViewModel vm)
                {
                    vm.Dispose.Execute(null);
                    ViewModel.Adressen.Value = App.Walter.Adressen.Select(a => new AdresseViewModel(a)).ToImmutableList();
                }
            }

        }

        private async void LoadDatabase_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var file = await Files.FilePicker(".db").PickSingleFileAsync();
            if (file != null)
            {
                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db");
                var stream = await file.OpenStreamForReadAsync();
                var bytes = new byte[(int)stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);

                var ok = Files.MakeSpace(path);
                if (ok)
                {
                    var folder = ApplicationData.Current.LocalFolder;
                    var newFile = await folder.CreateFileAsync("walter.db");
                    using (var writer = await newFile.OpenStreamForWriteAsync())
                    {
                        await writer.WriteAsync(bytes, 0, bytes.Length);
                    }

                    App.Walter.Dispose();
                    App.LoadDataBase();

                    ViewModel.LoadAdressen();
                }
            }
        }
    }
}
