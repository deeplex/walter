using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Services;
using Microsoft.UI.Xaml;
using SimpleInjector;

namespace Deeplex.Saverwalter.WinUI3
{
    sealed partial class App : Application
    {
        public static MainWindow Window { get; private set; }
        public static WalterDbService WalterService { get; private set; }
        public static FileService FileService { get; private set; }
        public static NotificationService NotificationService { get; private set; }
        public static Container Container { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            NotificationService = new NotificationService();
            WalterService = new WalterDbService(NotificationService);
            FileService = new FileService(WalterService);

            Container = new SimpleInjector.Container();
            Container.Register<INotificationService, NotificationService>();
            Container.Register<IWalterDbService, WalterDbService>();
            Container.Register<IFileService, FileService>();

            Container.Register<WohnungListViewModel>();
            //Container.Register<WohnungListViewModelEntry>();
            Container.Register<WohnungDetailViewModel>();

            Container.Register<VertragListViewModel>();
            //Container.Register<VertragListViewModelVertrag>(); // TODO rename to Entry
            //Container.Register<VertragListViewModelVertragVersion>();
            Container.Register<VertragDetailViewModel>();

            Container.Register<BetriebskostenRechnungenListViewModel>();
            //Container.Register<BetriebskostenRechnungenListViewModelEntry>();
            Container.Register<BetriebskostenrechnungDetailViewModel>();
            Container.Register<BetriebskostenrechnungPrintViewModel>();

            Container.Register<UmlageListViewModel>();
            //Container.Register<UmlageListViewModelEntry>();
            Container.Register<UmlageDetailViewModel>();

            Container.Register<KontaktListViewModel>();
            //Container.Register<KontaktListViewModelEntry>();
            Container.Register<NatuerlichePersonViewModel>(); // TODO add Detail to name
            Container.Register<JuristischePersonViewModel>(); // TODO add Detail to name

            Container.Register<ErhaltungsaufwendungenListViewModel>();
            ////Container.Register<ErhaltungsaufwendungenListViewModelEntry>();
            Container.Register<ErhaltungsaufwendungenDetailViewModel>();
            Container.Register<ErhaltungsaufwendungenPrintViewModel>();
            //Container.Register<ErhaltungsaufwendungenPrintEntry>();

            Container.Register<ZaehlerListViewModel>();
            //Container.Register<ZaehlerListViewModelEntry>();
            Container.Register<ZaehlerDetailViewModel>();

            Container.Register<ZaehlerstandListViewModel>();
            //Container.Register<ZaehlerstandListViewModelEntry>();

            // Only used in ViewModels (Through Vertrag):
            //Container.Register<MietenListViewModel>();
            //Container.Register<MietenListViewModelEntry>();
            //Container.Register<MietenDetailViewModel>();

            //Container.Register<MietMinderungListViewModel>();
            //Container.Register<MietminderungListViewModelEntry>();
            //Container.Register<MietMinderungDetailViewModel>();

            Container.Verify();

            Window = new MainWindow();
            Window.Activate();
        }
    }
}

