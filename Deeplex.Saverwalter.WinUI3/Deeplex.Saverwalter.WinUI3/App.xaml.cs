using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Utils;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Deeplex.Saverwalter.WinUI3
{
    sealed partial class App : Application
    {
        public static Window Window { get; private set; }
        public static AppViewModel ViewModel { get; private set; }

        public static SaverwalterContext Walter => ViewModel.ctx;
        public static void SaveWalter() => ViewModel.SaveWalter();

        public App()
        {
            // TODO: remove... This is to check where Walter has to go... Check todo in Model.cs aswell.
            var a = Package.Current.InstalledLocation.Path;
            var b = ApplicationData.Current.LocalFolder.Path;

            InitializeComponent();
        }

        public static async Task InitializeDatabase()
        {
            if (ViewModel.ctx != null) return;
            if (await ViewModel.Confirmation(
                "Noch keine Datenbank ausgewählt",
                "Datenbank suchen, oder leere Datenbank erstellen?",
                "Existierende Datenbank auswählen", "Erstelle neue leere Datenbank"))
            {
                await LoadDataBase();
            }
            else
            {
                // TODO
                throw new NotImplementedException();
            }
            ViewModel.loadAutoSuggestEntries();
        }

        public static async Task LoadDataBase()
        {
            try
            {
                await Saverwalter.ViewModels.Utils.Files.LoadDatabase(ViewModel);
            }
            catch
            {
                ViewModel?.ShowAlert("Konnte Datenbank nicht laden.");
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            ViewModel = new AppViewModel();

            Window = new MainWindow();
            Window.Activate();
        }
    }

    public sealed class AppViewModel : IAppImplementation
    {
        public void loadAutoSuggestEntries()
        {
            if (ctx != null)
            {
                AllAutoSuggestEntries = ctx.Wohnungen.Include(w => w.Adresse).Select(w => new AutoSuggestEntry(w)).ToList()
                        .Concat(ctx.NatuerlichePersonen.Select(w => new AutoSuggestEntry(w))).ToList()
                        .Concat(ctx.JuristischePersonen.Select(w => new AutoSuggestEntry(w))).ToList()
                        .Concat(ctx.Vertraege
                            .Include(w => w.Wohnung)
                            .Where(w => w.Ende == null || w.Ende < DateTime.Now)
                            .Select(w => new AutoSuggestEntry(w))).ToList()
                        .Concat(ctx.ZaehlerSet.Select(w => new AutoSuggestEntry(w))).ToList()
                        .Concat(ctx.Betriebskostenrechnungen.Include(w => w.Gruppen)
                            .ThenInclude(w => w.Wohnung).Select(w => new AutoSuggestEntry(w))).ToList()
                        .Where(w => w.Bezeichnung != null).ToImmutableList();
                AutoSuggestEntries.Value = AllAutoSuggestEntries;
            }
        }

        public void updateAutoSuggestEntries(string filter)
        {
            if (AllAutoSuggestEntries != null)
            {
                AutoSuggestEntries.Value = AllAutoSuggestEntries.Where(w => w.ToString().ToLower().Contains(filter.ToLower())).ToImmutableList();
            }
        }

        public ImmutableList<AutoSuggestEntry> AllAutoSuggestEntries;
        public ObservableProperty<ImmutableList<AutoSuggestEntry>> AutoSuggestEntries
            = new ObservableProperty<ImmutableList<AutoSuggestEntry>>();

        public AppViewModel()
        {
            Titel.Value = "Walter";
        }

        private CommandBar CommandBar { get; set; }

        public ObservableProperty<AnhangListViewModel> DetailAnhang
            = new ObservableProperty<AnhangListViewModel>();
        public ObservableProperty<AnhangListViewModel> ListAnhang
            = new ObservableProperty<AnhangListViewModel>();

        public void clearAnhang()
        {
            updateListAnhang(null);
            updateDetailAnhang(null);
        }
        public void updateListAnhang(AnhangListViewModel list) => updateAnhang(ListAnhang, list);
        public void updateDetailAnhang(AnhangListViewModel detail) => updateAnhang(DetailAnhang, detail);

        private void updateAnhang(ObservableProperty<AnhangListViewModel> op, AnhangListViewModel a)
        {
            op.Value = a;
        }

        public void RefillCommandContainer()
        {
            CommandBar.PrimaryCommands.Clear();
            CommandBar.SecondaryCommands.Clear();
        }

        public void RefillCommandContainer(IList<ICommandBarElement> Primary, IList<ICommandBarElement> Secondary = null)
        {
            RefillCommandContainer();
            foreach (var p in Primary)
            {
                CommandBar.PrimaryCommands.Add(p);
            }

            if (Secondary == null) return;

            foreach (var s in Secondary)
            {
                CommandBar.SecondaryCommands.Add(s);
            }
        }

        public void SetAnhangPane(SplitView arg, SymbolIcon arg2)
        {
            AnhangPane = arg;
            AnhangSymbol = arg2;
        }

        public void SetCommandBar(CommandBar arg)
        {
            CommandBar = arg;
        }

        public void SetConfirmationDialog(ContentDialog arg)
        {
            ConfirmationDialog = arg;
        }

        public void SetSavedIndicator(CommunityToolkit.WinUI.UI.Controls.InAppNotification arg, TextBlock arg2)
        {
            SavedIndicator = arg;
            SavedIndicatorText = arg2;
        }

        public Action<Type, object> Navigate { get; set; }

        // IAPPImplementation

        public string root { get; set; }
        public SaverwalterContext ctx { get; set; }
        public ObservableProperty<string> Titel { get; set; } = new ObservableProperty<string>();
        private CommunityToolkit.WinUI.UI.Controls.InAppNotification SavedIndicator { get; set; }
        private TextBlock SavedIndicatorText { get; set; }
        private ContentDialog ConfirmationDialog { get; set; }
        private SplitView AnhangPane { get; set; }
        private SymbolIcon AnhangSymbol { get; set; }

        public async Task<string> pickFile()
        {
            var picker = Files.FilePicker(".db");
            var picked = await picker.PickSingleFileAsync();

            return picked.Path;
        }

        public async Task<List<string>> pickFiles()
        {
            var picker = Files.FilePicker("*");
            var files = await picker.PickMultipleFilesAsync();

            return files.Select(f => f.Path).ToList();
        }

        public async Task deleteFile(Anhang a)
        {
            if (await Confirmation())
            {
                ctx.Anhaenge.Remove(a);
                SaveWalter();

                File.Delete(a.getPath(root));
            }
        }

        public async void launchFile(Anhang a)
        {
            try
            {
                var path = a.getPath(root);
                var file = await StorageFile.GetFileFromPathAsync(path);
                await Windows.System.Launcher.LaunchFileAsync(file);
            }
            catch (Exception e)
            {
                ShowAlert(e.Message);
            }
        }

        public void SaveWalter()
        {
            ctx.SaveChanges();
            ShowAlert("Gespeichert", 1000);
        }

        public async Task<bool> Confirmation()
        {
            SetConfirmationDialogText(
                "Bist du sicher?",
                "Diese Änderung kann nicht rückgängig gemacht werden.",
                "Ja", "Nein");
            return await ShowConfirmationDialog();
        }
        public async Task<bool> Confirmation(string title, string content, string primary, string secondary)
        {
            if (ConfirmationDialog == null)
            {
                throw new Exception("Confirmation dialog is not defined.");
            }
            SetConfirmationDialogText(title, content, primary, secondary);
            return await ShowConfirmationDialog();
        }

        public void ShowAlert(string text) => ShowAlert(text, 500);
        public void ShowAlert(string text, int ms)
        {
            if (SavedIndicator == null)
            {
                return;
            }
            SavedIndicatorText.Text = text;
            SavedIndicator.Show(ms);
        }

        public void OpenAnhangPane()
        {
            if (!AnhangPane.IsPaneOpen)
            {
                ToggleAnhang();
            }
        }

        public void ToggleAnhang()
        {
            AnhangPane.IsPaneOpen = !AnhangPane.IsPaneOpen;
            AnhangSymbol.Symbol = AnhangPane.IsPaneOpen ? Symbol.OpenPane : Symbol.ClosePane;
        }

        private async Task<bool> ShowConfirmationDialog()
            => await ConfirmationDialog.ShowAsync() == ContentDialogResult.Primary;

        private void SetConfirmationDialogText(string title, string content, string primary, string secondary)
        {
            if (ConfirmationDialog != null)
            {
                ConfirmationDialog.Title = title;
                ConfirmationDialog.Content = content;
                ConfirmationDialog.PrimaryButtonText = primary;
                ConfirmationDialog.SecondaryButtonText = secondary;
            }
        }
    }
}

