using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.App
{
    sealed partial class App : Application
    {
        public static AppViewModel ViewModel { get; private set; }

        public static SaverwalterContext Walter => ViewModel.ctx;
        public static void SaveWalter() => ViewModel.SaveWalter();

        public App()
        {
            // TODO: remove... This is to check where Walter has to go... Check todo in Model.cs aswell.
            var a = Package.Current.InstalledLocation.Path;
            var b = ApplicationData.Current.LocalFolder.Path;

            InitializeComponent();
            Suspending += OnSuspending;
        }

        public static async Task CopyDataBase()
        {
            var picker = Files.FilePicker(".db");
            picker.CommitButtonText = "Lade Datenbank";
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db");
                var stream = await file.OpenStreamForReadAsync();
                var bytes = new byte[(int)stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);

                var ok = ViewModels.Utils.Anhaenge.MakeSpace(path);
                if (ok)
                {
                    var folder = ApplicationData.Current.LocalFolder;
                    var newFile = await folder.CreateFileAsync("walter.db");
                    using (var writer = await newFile.OpenStreamForWriteAsync())
                    {
                        await writer.WriteAsync(bytes, 0, bytes.Length);
                    }

                    if (ViewModel.ctx != null)
                    {
                        ViewModel.ctx.Dispose();
                    }
                }
            }
            LoadDataBase();
        }

        public static async Task InitializeDatabase()
        {
            if (ViewModel.ctx != null) return;
            if (File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db")))
            {
                ViewModel.ctx?.Dispose();
                LoadDataBase();
            }
            else if (await ViewModel.Confirmation(
                "Keine Datenbank gefunden",
                "Datenbank suchen, oder leere Datenbank erstellen?",
                "Existierende Datenbank auswählen", "Erstelle neue leere Datenbank"))
            {
                await CopyDataBase();
            }
            else
            {
                ViewModel.ctx?.Dispose();
                LoadDataBase();
            }
        }

        public static void LoadDataBase()
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
                optionsBuilder.UseSqlite("Data Source=" + Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db"));
                ViewModel.ctx = new SaverwalterContext(optionsBuilder.Options);
                ViewModel.ctx.Database.Migrate();
            }
            catch
            {
                try
                {
                    ViewModel?.ShowAlert("Konnte Datenbank nicht laden.");
                }
                catch { }
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            ViewModel = new AppViewModel();

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }

    public abstract partial class AppImplementation : IAppImplementation
    {
        public SaverwalterContext ctx { get; set; }
        public ObservableProperty<string> Titel { get; set; } = new ObservableProperty<string>();
        protected InAppNotification SavedIndicator { get; set; }
        protected TextBlock SavedIndicatorText { get; set; }
        protected ContentDialog ConfirmationDialog { get; set; }
        protected SplitView AnhangPane { get; set; }
        protected SymbolIcon AnhangSymbol { get; set; }

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
            SetConfirmationDialogText(title, content, primary, secondary);
            return await ShowConfirmationDialog();
        }
        public void ShowAlert(string text, int ms = 500)
        {
            if (SavedIndicator == null)
            {
                return;
            }
            SavedIndicatorText.Text = text;
            SavedIndicator.Show(ms);
        }

        public void OpenAnhang()
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
            ConfirmationDialog.Title = title;
            ConfirmationDialog.Content = content;
            ConfirmationDialog.PrimaryButtonText = primary;
            ConfirmationDialog.SecondaryButtonText = secondary;
        }
    }

    public sealed class AppViewModel : AppImplementation
    {
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

        public void SetSavedIndicator(InAppNotification arg, TextBlock arg2)
        {
            SavedIndicator = arg;
            SavedIndicatorText = arg2;
        }

        public Action<Type, object> Navigate { get; set; }
    }
}
