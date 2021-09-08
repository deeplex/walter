using Deeplex.Saverwalter.App.ViewModels.Utils;
using Deeplex.Saverwalter.App.Views;
using Deeplex.Saverwalter.App.Views.Rechnungen;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App
{
    public sealed partial class MainPage : Page
    {
        public void Navigate<U>(Type SourcePage, U SendParameter)
        {
            ViewModel.clearAnhang();

            AppFrame.Navigate(SourcePage, SendParameter,
                new DrillInNavigationTransitionInfo());
        }

        public MainPage()
        {
            InitializeComponent();

            ViewModel.SetCommandBar(MainCommandBar);
            ViewModel.SetAnhangPane(splitview, togglepanesymbol);
            ViewModel.SetSavedIndicator(SavedIndicator, SavedIncdicatorText);
            ViewModel.SetConfirmationDialog(ConfirmationDialog);
            ViewModel.Navigate = Navigate;
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

        private void togglepane_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.ViewModel.ToggleAnhang();
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            App.ViewModel.updateAutoSuggestEntries(sender.Text);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var Entry = args.ChosenSuggestion as AutoSuggestEntry;
            App.ViewModel.Navigate(Entry.getPage(), Entry.Entity);
            sender.IsSuggestionListOpen = false;
            sender.Text = "";
        }
    }



    public class AutoSuggestEntry
    {
        public override string ToString() => Bezeichnung;
        public string Bezeichnung;
        public string Icon;
        public object Entity;


        public Type getPage()
        {
            return Entity is NatuerlichePerson ? typeof(NatuerlichePersonDetailPage) :
                Entity is JuristischePerson ? typeof(JuristischePersonenDetailPage) :
                Entity is Wohnung ? typeof(WohnungDetailPage) :
                Entity is Zaehler ? typeof(ZaehlerDetailPage) :
                Entity is Vertrag ? typeof(VertragDetailViewPage) :
                Entity is Betriebskostenrechnung ? typeof(BetriebskostenrechnungenDetailPage) :
                Entity is Erhaltungsaufwendung ? typeof(ErhaltungsaufwendungenDetailPage) :
                null;
        }

        public AutoSuggestEntry(NatuerlichePerson a)
        {
            Entity = a;
            Icon = "ContactInfo";
            Bezeichnung = a.Bezeichnung;
        }
        public AutoSuggestEntry(JuristischePerson a)
        {
            Entity = a;
            Icon = "ContactInfo";
            Bezeichnung = a.Bezeichnung;
        }
        public AutoSuggestEntry(Wohnung a)
        {
            Entity = a;
            Icon = "Street";
            Bezeichnung = AdresseViewModel.Anschrift(a) + " - " + a.Bezeichnung;
        }
        public AutoSuggestEntry(Zaehler a)
        {
            Entity = a;
            Icon = "Clock";
            Bezeichnung = a.Kennnummer;
        }
        public AutoSuggestEntry(Vertrag a)
        {
            Entity = a;
            Icon = "Library";
            Bezeichnung = AdresseViewModel.Anschrift(a.Wohnung) + " - " + a.Wohnung.Bezeichnung;
        }
        public AutoSuggestEntry(Betriebskostenrechnung a)
        {
            Entity = a;
            Icon = "List";
            Bezeichnung = a.Typ.ToDescriptionString() + " - " + a.GetWohnungenBezeichnung(App.ViewModel);
        }
        public AutoSuggestEntry(Erhaltungsaufwendung a)
        {
            Entity = a;
            Icon = "Bullets";
            Bezeichnung = a.Bezeichnung;
        }
    }
}
