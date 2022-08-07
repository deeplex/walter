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
            Container.Register<INotificationService, NotificationService>(Lifestyle.Singleton);
            Container.Register<IWalterDbService, WalterDbService>(Lifestyle.Singleton);
            Container.Register<IFileService, FileService>(Lifestyle.Singleton);

            Container.Register<WohnungListViewModel>();

            Window = new MainWindow();
            Window.Activate();
        }
    }
}

