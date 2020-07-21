using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

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
            Explorer.Navigate(SendParameter);
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
