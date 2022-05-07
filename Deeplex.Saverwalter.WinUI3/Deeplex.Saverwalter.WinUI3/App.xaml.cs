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
using Deeplex.Saverwalter.WinUI3.Services;

namespace Deeplex.Saverwalter.WinUI3
{
    sealed partial class App : Application
    {
        public static MainWindow Window { get; private set; }
        public static WalterDbService WalterService { get; private set; }
        public static FileService FileService { get; private set; }
        public static NotificationService NotificationService { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            NotificationService = new NotificationService();
            WalterService = new WalterDbService(NotificationService);
            FileService = new FileService(WalterService);

            Window = new MainWindow();
            Window.Activate();
        }
    }
}

