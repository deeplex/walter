using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        public ContentDialog ConfirmationDialog => confirmationDialog;
        public InAppNotification Alert => alert;
        public TextBlock AlertText => alertText;

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

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.initializeDatabase(App.Impl);
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

        private void togglepane_Click(object sender, RoutedEventArgs e)
        {
            ToggleAnhang();
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

        public void RefillCommandContainer()
        {
            MainCommandBar.PrimaryCommands.Clear();
            MainCommandBar.SecondaryCommands.Clear();
        }

        public void RefillCommandContainer(IList<ICommandBarElement> Primary, IList<ICommandBarElement> Secondary = null)
        {
            RefillCommandContainer();
            foreach (var p in Primary)
            {
                MainCommandBar.PrimaryCommands.Add(p);
            }

            if (Secondary == null) return;

            foreach (var s in Secondary)
            {
                MainCommandBar.SecondaryCommands.Add(s);
            }
        }

        public void OpenAnhangPane()
        {
            if (!splitview.IsPaneOpen)
            {
                ToggleAnhang();
            }
        }

        public void ToggleAnhang()
        {
            splitview.IsPaneOpen = !splitview.IsPaneOpen;
            togglepanesymbol.Symbol = splitview.IsPaneOpen ? Symbol.OpenPane : Symbol.ClosePane;
        }
    }
}
