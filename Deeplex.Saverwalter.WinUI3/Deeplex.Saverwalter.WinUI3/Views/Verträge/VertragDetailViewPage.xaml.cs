using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class VertragDetailViewPage : Page
    {
        public VertragDetailViewModel ViewModel { get; set; }
        public BetriebskostenRechnungenListViewModel BetriebskostenListViewModel { get; private set; }
        public UmlageListViewModel UmlageListViewModel { get; private set; }

        public VertragDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Vertrag v)
            {
                ViewModel = App.Container.GetInstance<VertragDetailViewModel>();
                ViewModel.SetEntity(v);
                BetriebskostenListViewModel = App.Container.GetInstance<BetriebskostenRechnungenListViewModel>();
                BetriebskostenListViewModel.SetList(v);
                UmlageListViewModel = App.Container.GetInstance<UmlageListViewModel>();
                UmlageListViewModel.SetList(v);
            }
            else
            {
                ViewModel = new VertragDetailViewModel(App.NotificationService, App.WalterService);
            }

            App.Window.CommandBar.MainContent = new UserControls.DetailCommandBarControl<Vertrag>() { ViewModel = ViewModel };
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
