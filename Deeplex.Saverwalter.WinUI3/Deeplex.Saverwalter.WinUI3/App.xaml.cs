using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Utils;
using Deeplex.Saverwalter.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.WinUI3
{
    sealed partial class App : Application, IAppImplementation
    {
        public static MainWindow Window { get; private set; }
        public static WalterDbService WalterService { get; private set; }

        public static IAppImplementation Impl => Current as IAppImplementation;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var self = this;
            WalterService = new WalterDbService(self);

            Window = new MainWindow();
            Window.Activate();
        }

        public async Task<string> saveFile(string filename, string[] ext)
        {
            var filetypes = ext.Length == 0 ? new string[] { "*" } : ext;
            var picker = Files.FileSavePicker(filename, filetypes);
            var picked = await picker.PickSaveFileAsync();

            return picked?.Path;
        }

        public async Task<string> pickFile(params string[] ext)
        {
            var filetypes = ext.Length == 0 ? new string[] { "*" } : ext;
            var picker = Files.FileOpenPicker(filetypes);
            var picked = await picker.PickSingleFileAsync();

            return picked?.Path;
        }

        public async Task<List<string>> pickFiles(params string[] ext)
        {
            var filetypes = ext.Length == 0 ? new string[] { "*" } : ext;
            var picker = Files.FileOpenPicker(filetypes);
            var files = await picker.PickMultipleFilesAsync();

            return files.Select(f => f.Path).ToList();
        }

        public async void launchFile(Anhang a)
        {
            try
            {
                var path = a.getPath(WalterService.root);
                var file = await StorageFile.GetFileFromPathAsync(path);
                await Windows.System.Launcher.LaunchFileAsync(file);
            }
            catch (Exception e)
            {
                ShowAlert(e.Message);
            }
        }

        public void ShowAlert(string text) => ShowAlert(text, text.Length > 20 ? 0 : 500);
        public void ShowAlert(string text, int ms)
        {
            if (Window.Alert == null)
            {
                return;
            }
            Window.AlertText.Text = text;
            Window.Alert.Show(ms);
        }

        public async Task<bool> Confirmation(string title, string content, string primary, string secondary)
        {
            SetConfirmationDialogText(title, content, primary, secondary);
            return await ShowConfirmationDialog();
        }
        public async Task<bool> Confirmation()
        {
            SetConfirmationDialogText(
                "Bist du sicher?",
                "Diese Änderung kann nicht rückgängig gemacht werden.",
                "Ja", "Nein");
            return await ShowConfirmationDialog();
        }

        private async Task<bool> ShowConfirmationDialog()
            => await Window.ConfirmationDialog.ShowAsync() == ContentDialogResult.Primary;

        private void SetConfirmationDialogText(string title, string content, string primary, string secondary)
        {
            if (Window.ConfirmationDialog is ContentDialog wcd)
            {
                wcd.Title = title;
                wcd.Content = content;
                wcd.PrimaryButtonText = primary;
                wcd.SecondaryButtonText = secondary;
            }
        }
    }
}

