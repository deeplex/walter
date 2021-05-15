using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.App.ViewModels;
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

namespace Deeplex.Saverwalter.App
{
    sealed partial class App : Application
    {
        public static MainViewModel ViewModel { get; private set; }

        public static SaverwalterContext Walter { get; private set; }

        public static void SaveWalter()
        {
            Walter.SaveChanges();
            ViewModel.ShowAlert("Gespeichert", 1000);
        }

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

                var ok = Files.MakeSpace(path);
                if (ok)
                {
                    var folder = ApplicationData.Current.LocalFolder;
                    var newFile = await folder.CreateFileAsync("walter.db");
                    using (var writer = await newFile.OpenStreamForWriteAsync())
                    {
                        await writer.WriteAsync(bytes, 0, bytes.Length);
                    }

                    if (Walter != null)
                    {
                        Walter.Dispose();
                    }
                }
            }
            LoadDataBase();
        }

        public static async Task InitializeDatabase()
        {
            if (Walter != null) return;
            if (File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db")))
            {
                Walter?.Dispose();
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
                Walter?.Dispose();
                LoadDataBase();
            }
        }

        public static void LoadDataBase()
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
                optionsBuilder.UseSqlite("Data Source=" + Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db"));
                Walter = new SaverwalterContext(optionsBuilder.Options);
                Walter.Database.Migrate();
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

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            ViewModel = new MainViewModel();
            
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
}
