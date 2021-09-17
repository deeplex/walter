using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        public ContentDialog ConfirmationDialog => confirmationDialog;
        public InAppNotification Alert => alert;
        public TextBlock AlertText => alertText;

        public CommandBarControl CommandBar => commandBar;
        public SplitView SplitView => splitview;

        public void Navigate<U>(Type SourcePage, U SendParameter)
        {
            ViewModel.clearAnhang();

            AppFrame.Navigate(SourcePage, SendParameter,
                new DrillInNavigationTransitionInfo());
        }

        public MainWindow()
        {
            InitializeComponent();

            root.Loaded += Root_Loaded;
        }

        private async void Root_Loaded(object sender, RoutedEventArgs e)
        {
            var Settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var str = Settings.Values["root"] as string;
            App.ViewModel.root = str;
            await ViewModel.initializeDatabase(App.Impl);

            if (str != App.ViewModel.root)
            {
                Utils.Elements.SetDatabaseAsDefault();
            }
        }

        public Frame AppFrame => frame;
        public AppViewModel ViewModel = App.ViewModel;

        public readonly string KontaktListLabel = "Kontakte";
        public readonly string VertragListLabel = "Verträge";
        public readonly string WohnungListLabel = "Mietobjekte";
        public readonly string ZaehlerListLabel = "Zähler";
        public readonly string BetriebskostenrechnungenListLabel = "Betr. Rechnung";
        public readonly string ErhaltungsaufwendungenListLabel = "Erhaltungsaufw.";

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
                label == ErhaltungsaufwendungenListLabel ? typeof(ErhaltungsaufwendungenListViewPage) :
                null;

            if (pageType != null && pageType != AppFrame.CurrentSourcePageType)
            {
                Navigate(pageType, label);
            }
        }

        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.SourcePageType == typeof(KontaktListPage) ||
                e.SourcePageType == typeof(JuristischePersonenDetailPage) ||
                e.SourcePageType == typeof(NatuerlichePersonDetailPage))
            {
                NavView.SelectedItem = KontaktListMenuItem;
            }
            else if (e.SourcePageType == typeof(WohnungListPage) || e.SourcePageType == typeof(WohnungDetailPage))
            {
                NavView.SelectedItem = WohnungListMenuItem;
            }
            else if (e.SourcePageType == typeof(VertragListPage) || e.SourcePageType == typeof(VertragDetailViewPage))
            {
                NavView.SelectedItem = VertragListMenuItem;
            }
            else if (e.SourcePageType == typeof(BetriebskostenRechnungenListViewPage) || e.SourcePageType == typeof(BetriebskostenrechnungenDetailPage))
            {
                NavView.SelectedItem = BetriebskostenListMenuItem;
            }
            else if (e.SourcePageType == typeof(ErhaltungsaufwendungenListViewPage) || e.SourcePageType == typeof(ErhaltungsaufwendungenDetailPage))
            {
                NavView.SelectedItem = ErhaltungsAufwendungenListMenuItem;
            }
            else if (e.SourcePageType == typeof(ZaehlerListPage) || e.SourcePageType == typeof(ZaehlerDetailPage))
            {
                NavView.SelectedItem = ZaehlerListMenuItem;
            }
            else if (e.SourcePageType == typeof(SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            if (e.NavigationMode == NavigationMode.Back)
            {
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

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            App.ViewModel.updateAutoSuggestEntries(sender.Text);
        }

        private Type GetEntryPage(object Entry)
        {
            return Entry is NatuerlichePerson ? typeof(NatuerlichePersonDetailPage) :
                Entry is JuristischePerson ? typeof(JuristischePersonenDetailPage) :
                Entry is Wohnung ? typeof(WohnungDetailPage) :
                Entry is Zaehler ? typeof(ZaehlerDetailPage) :
                Entry is Vertrag ? typeof(VertragDetailViewPage) :
                Entry is Betriebskostenrechnung ? typeof(BetriebskostenrechnungenDetailPage) :
                Entry is Erhaltungsaufwendung ? typeof(ErhaltungsaufwendungenDetailPage) :
                null;
        }
        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var Entry = args.ChosenSuggestion as AutoSuggestEntry;
            Navigate(GetEntryPage(Entry), Entry.Entity);
            sender.IsSuggestionListOpen = false;
            sender.Text = "";
        }
    }
}
