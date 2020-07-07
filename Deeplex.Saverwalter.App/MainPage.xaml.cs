﻿using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Deeplex.Saverwalter.App.ViewModels.AnhangViewModel;

namespace Deeplex.Saverwalter.App
{
    public sealed partial class MainPage : Page
    {
        public AnhangViewModel Explorer => App.ViewModel.Explorer.Value;

        private sealed class ExplorerItemTemplateSelector : DataTemplateSelector
        {
            public DataTemplate AnhangTemplate { get; private set; }
            public DataTemplate OtherTemplate { get; private set; }

            public ExplorerItemTemplateSelector(DataTemplate A, DataTemplate O)
            {
                AnhangTemplate = A;
                OtherTemplate = O;
            }

            protected override DataTemplate SelectTemplateCore(object item)
            {
                return item is AnhangDatei ? AnhangTemplate : OtherTemplate;
            }
        }

        public MainPage()
        {
            InitializeComponent();

            ViewModel.SetCommandBar(MainCommandBar);
            App.ViewModel.Explorer.Value = new AnhangViewModel(ExplorerTree);
            ExplorerTree.ItemTemplateSelector = new ExplorerItemTemplateSelector(AnhangTemplate, OtherTemplate);
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
