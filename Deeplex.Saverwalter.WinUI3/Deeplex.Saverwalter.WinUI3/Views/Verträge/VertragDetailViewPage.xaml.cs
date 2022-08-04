using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class VertragDetailViewPage : Page
    {
        public VertragDetailViewModel ViewModel { get; set; }
        public KontaktListViewModel MieterListViewModel { get; private set; }
        public BetriebskostenRechnungenListViewModel BetriebskostenListViewModel { get; private set; }
        public MietenListViewModel MietenListViewModel { get; private set; }
        public MietMinderungListViewModel MietMinderungListViewModel { get; private set; }

        public VertragDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Vertrag v)
            {
                ViewModel = new(v.VertragId, App.NotificationService, App.WalterService);
                MieterListViewModel = new(App.WalterService, App.NotificationService, v);
                BetriebskostenListViewModel = new(App.WalterService, App.NotificationService, v);
                MietenListViewModel = new(v.VertragId, App.NotificationService, App.WalterService);
                MietMinderungListViewModel = new(v.VertragId, App.NotificationService, App.WalterService);
            }
            else
            {
                ViewModel = new VertragDetailViewModel(App.NotificationService, App.WalterService);
            }

            App.Window.CommandBar.MainContent = new UserControls.DetailCommandBarControl() { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }

        private void Betriebskostenabrechnung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(BetriebskostenrechnungPrintViewPage), ViewModel.Entity,
                new DrillInNavigationTransitionInfo());
        }
    }
}
