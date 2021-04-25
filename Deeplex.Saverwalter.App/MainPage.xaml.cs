using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App
{
    public sealed partial class MainPage : Page
    {
        public AnhangViewModel Explorer => App.ViewModel.Explorer.Value;

        private sealed class ExplorerItemTemplateSelector : DataTemplateSelector
        {
            public DataTemplate AnhangTemplate { get; private set; }
            public DataTemplate GroupTemplate { get; private set; }
            public DataTemplate RootTemplate { get; private set; }

            public ExplorerItemTemplateSelector(DataTemplate A, DataTemplate O, DataTemplate R)
            {
                AnhangTemplate = A;
                GroupTemplate = O;
                RootTemplate = R;
            }

            protected override DataTemplate SelectTemplateCore(object item)
            {
                return
                    item is AnhangDatei ? AnhangTemplate :
                    item is AnhangTreeViewNode ? GroupTemplate :
                    RootTemplate;
            }
        }

        public void Navigate<U>(Type SourcePage, U SendParameter)
        {
            if (Explorer.navigationSynced.Value)
            {
                Explorer.Navigate(SendParameter);
            }
            AppFrame.Navigate(SourcePage, SendParameter,
                new DrillInNavigationTransitionInfo());
        }

        public MainPage()
        {
            InitializeComponent();

            ViewModel.SetCommandBar(MainCommandBar);
            ViewModel.SetSavedIndicator(SavedIndicator);
            ViewModel.Navigate = Navigate;
            App.ViewModel.Explorer.Value = new AnhangViewModel(ExplorerTree);
            ExplorerTree.ItemTemplateSelector = new ExplorerItemTemplateSelector(AnhangTemplate, GroupTemplate, RootTemplate);
        }

        public Frame AppFrame => frame;
        public MainViewModel ViewModel = App.ViewModel;

        public readonly string KontaktListLabel = "Kontakte";
        public readonly string VertragListLabel = "Verträge";
        public readonly string WohnungListLabel = "Mietobjekte";
        public readonly string ZaehlerListLabel = "Zähler";
        public readonly string BetriebskostenrechnungenListLabel = "Betr. Rechnung";

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var label = args.InvokedItem as string;
            var pageType =
                args.IsSettingsInvoked ? typeof(SettingsPage) :
                label == KontaktListLabel ? typeof(KontaktListPage) :
                label == WohnungListLabel ? typeof(WohnungListPage) :
                label == VertragListLabel ? typeof(VertragListPage) :
                label == ZaehlerListLabel ? typeof(ZaehlerListPage) :
                label == BetriebskostenrechnungenListLabel ? typeof(BetriebskostenRechnungenListViewPage) :
                null;

            if (pageType != null && pageType != AppFrame.CurrentSourcePageType)
            {
                Navigate(pageType, label);
            }
        }

        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (e.SourcePageType == typeof(KontaktListPage))
                {
                    NavView.SelectedItem = KontaktListMenuItem;
                }
                else if (e.SourcePageType == typeof(WohnungListPage))
                {
                    NavView.SelectedItem = WohnungListMenuItem;
                }
                else if (e.SourcePageType == typeof(VertragListPage))
                {
                    NavView.SelectedItem = VertragListMenuItem;
                }
                else if (e.SourcePageType == typeof(BetriebskostenRechnungenListViewPage))
                {
                    NavView.SelectedItem = BetriebskostenListMenuItem;
                }
                else if (e.SourcePageType == typeof(ZaehlerListPage))
                {
                    NavView.SelectedItem = ZaehlerListMenuItem;
                }
                else if (e.SourcePageType == typeof(SettingsPage))
                {
                    NavView.SelectedItem = NavView.SettingsItem;
                }
                Navigate(e.SourcePageType, (NavView.SelectedItem as NavigationViewItem).Content);
            }
        }


        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
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
