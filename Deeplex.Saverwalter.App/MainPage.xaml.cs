using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using System;
using System.Collections.Immutable;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            ExplorerMenuBar.Items.Add(ViewModel.Anhang.AnhangKontaktList);
            ExplorerMenuBar.Items.Add(ViewModel.Anhang.AnhangAdresseList);
            ExplorerMenuBar.Items.Add(ViewModel.Anhang.AnhangVertragList);
            ViewModel.Anhang.BreadCrumbs = ExplorerBreadCrumbs;
        }

        public Frame AppFrame => frame;
        public SplitView AppSplitView => splitview;
        public MainViewModel ViewModel = new MainViewModel();

        public readonly string KontaktListLabel = "Kontakte";
        public readonly string VertragListLabel = "Verträge";
        public readonly string WohnungListLabel = "Mietobjekte";
        public readonly string BetriebskostenrechnungenListLabel = "Betr. Rechnung";

        private void NavView_ItemInvoked(Windows.UI.Xaml.Controls.NavigationView sender, Windows.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            var label = args.InvokedItem as string;
            var pageType =
                args.IsSettingsInvoked ? typeof(SettingsPage) :
                label == KontaktListLabel ? typeof(KontaktListPage) :
                label == WohnungListLabel ? typeof(WohnungListPage) :
                label == VertragListLabel ? typeof(VertragListPage) :
                label == BetriebskostenrechnungenListLabel ? typeof(BetriebskostenRechnungenTypListViewPage) :
                null;
            if (pageType != null && pageType != AppFrame.CurrentSourcePageType)
            {
                AppFrame.Navigate(pageType);
            }
        }

        private void NavView_BackRequested(Windows.UI.Xaml.Controls.NavigationView sender, Windows.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            if (AppFrame.CanGoBack)
            {
                AppFrame.GoBack();
            }
        }

        private void togglepane_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            splitview.IsPaneOpen = !splitview.IsPaneOpen;
            togglepanesymbol.Symbol = splitview.IsPaneOpen ? Symbol.OpenPane : Symbol.ClosePane;
        }
    }
}
