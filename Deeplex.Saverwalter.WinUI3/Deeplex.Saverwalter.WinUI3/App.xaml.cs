using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using SimpleInjector;
using System.IO;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.WinUI3
{
    sealed partial class App : Application
    {
        public static MainWindow Window { get; private set; }
        public static Container Container { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {

            Container = new SimpleInjector.Container();
            Container.Register<INotificationService, NotificationService>();
            Container.Register<IFileService, FileService>();
            Container.Register<IWalterDbService, WalterDbService>();

            Container.Register<SettingsViewModel>();

            Container.Register<AnhangListViewModel>();

            Container.Register<WohnungListViewModel>();
            Container.Register<WohnungDetailViewModel>();

            Container.Register<VertragListViewModel>();
            Container.Register<VertragDetailViewModel>();

            Container.Register<BetriebskostenRechnungenListViewModel>();
            Container.Register<BetriebskostenrechnungDetailViewModel>();
            Container.Register<BetriebskostenrechnungPrintViewModel>();

            Container.Register<UmlageListViewModel>();
            Container.Register<UmlageDetailViewModel>();

            Container.Register<KontaktListViewModel>();
            Container.Register<NatuerlichePersonViewModel>(); // TODO add Detail to name
            Container.Register<JuristischePersonViewModel>(); // TODO add Detail to name

            Container.Register<ErhaltungsaufwendungenListViewModel>();
            Container.Register<ErhaltungsaufwendungenDetailViewModel>();
            Container.Register<ErhaltungsaufwendungenPrintViewModel>();

            Container.Register<ZaehlerListViewModel>();
            Container.Register<ZaehlerDetailViewModel>();

            // Missing:
            // Container.Register<WohnungListViewModelEntry>();
            // Container.Register<VertragListViewModelVertrag>(); // TODO rename to Entry
            // Container.Register<VertragListViewModelVertragVersion>();
            // Container.Register<BetriebskostenRechnungenListViewModelEntry>();
            // Container.Register<UmlageListViewModelEntry>();
            // Container.Register<KontaktListViewModelEntry>();
            // Container.Register<ErhaltungsaufwendungenListViewModelEntry>();
            // Container.Register<ErhaltungsaufwendungenPrintEntry>();
            // Container.Register<ZaehlerListViewModelEntry>();
            // Container.Register<ZaehlerstandListViewModel>();
            // Container.Register<ZaehlerstandListViewModelEntry>();
            // Container.Register<MietenListViewModel>();
            // Container.Register<MietenListViewModelEntry>();
            // Container.Register<MietenDetailViewModel>();
            // Container.Register<MietMinderungListViewModel>();
            // Container.Register<MietminderungListViewModelEntry>();
            // Container.Register<MietMinderungDetailViewModel>();
            Container.Verify();

            await initializeDatabase();


            Window = new MainWindow();
            Window.Activate();
        }

        private async Task initializeDatabase()
        {
            // Removed when moving from MainWindow to App
            // AutoSuggest = new AutoSuggestListViewModel(App.WalterService);

            var db = App.Container.GetInstance<IWalterDbService>();
            var fs = App.Container.GetInstance<IFileService>();
            var ns = App.Container.GetInstance<INotificationService>();

            if (fs.databaseRoot == null || !File.Exists(fs.databaseRoot + ".db"))
            {
                var path = await ns.Confirmation(
                    "Noch keine Datenbank ausgewählt",
                    "Datenbank suchen, oder leere Datenbank erstellen?",
                    "Existierende Datenbank auswählen", "Erstelle neue leere Datenbank") ?
                        await fs.pickFile(".db") :
                        await fs.saveFile("walter", new string[] { ".db" });
                var databaseRoot = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                Utils.Elements.SetDatabaseAsDefault(databaseRoot);
            }

            //if (db.ctx != null)
            //{
            //    db.ctx.Dispose();
            //}

            db.ctx.Database.Migrate();
        }
    }
}

