using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        public ContentDialog ConfirmationDialog => confirmationDialog;
        public InAppNotification Alert => alert;
        public TextBlock AlertText => alertText;

        public CommandBarControl CommandBar => commandBar;
        public SplitView SplitView => splitview;

        public ObservableProperty<string> Titel = new();
        public AutoSuggestListViewModel AutoSuggest { get; private set; }

        // DetailAnhang is the list of Anhaenge for a View which is the current Main Window
        public ObservableProperty<AnhangListViewModel> DetailAnhang { get; private set; } = new();
        // ListAnhang is to be filled using GridViews. TODO #17
        public ObservableProperty<AnhangListViewModel> ListAnhang { get; private set; } = new();

        public async void Navigate<U>(Type SourcePage, U SendParameter)
        {
            if (await checkOutOfSync())
            {
                return;
            }

            DetailAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            if (SendParameter is IAnhang a)
            {
                DetailAnhang.Value.SetList(a);
            }
            ListAnhang.Value = null;

            AppFrame.Navigate(SourcePage, SendParameter,
                new DrillInNavigationTransitionInfo());
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public Frame AppFrame => frame;

        public readonly string KontaktListLabel = "Kontakte";
        public readonly string VertragListLabel = "Verträge";
        public readonly string WohnungListLabel = "Mietobjekte";
        public readonly string ZaehlerListLabel = "Zähler";
        public readonly string BetriebskostenrechnungListLabel = "Betr. Rechnung";
        public readonly string ErhaltungsaufwendungListLabel = "Erhaltungsaufw.";
        public readonly string UmlageListLabel = "Umlagen";

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var label = args.InvokedItem as string;
            var pageType =
                args.IsSettingsInvoked ? typeof(SettingsViewPage) :
                label == KontaktListLabel ? typeof(KontaktListViewPage) :
                label == WohnungListLabel ? typeof(WohnungListViewPage) :
                label == VertragListLabel ? typeof(VertragListViewPage) :
                label == ZaehlerListLabel ? typeof(ZaehlerListViewPage) :
                label == BetriebskostenrechnungListLabel ? typeof(BetriebskostenrechnungListViewPage) :
                label == ErhaltungsaufwendungListLabel ? typeof(ErhaltungsaufwendungListViewPage) :
                label == UmlageListLabel ? typeof(UmlageListViewPage) :
                null;

            if (pageType != null && pageType != AppFrame.CurrentSourcePageType)
            {
                Navigate(pageType, label);
            }
        }

        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.SourcePageType == typeof(KontaktListViewPage) ||
                e.SourcePageType == typeof(JuristischePersonDetailViewPage) ||
                e.SourcePageType == typeof(NatuerlichePersonDetailViewPage))
            {
                NavView.SelectedItem = KontaktListMenuItem;
            }
            else if (e.SourcePageType == typeof(WohnungListViewPage) || e.SourcePageType == typeof(WohnungDetailViewPage))
            {
                NavView.SelectedItem = WohnungListMenuItem;
            }
            else if (e.SourcePageType == typeof(VertragListViewPage) || e.SourcePageType == typeof(VertragDetailViewPage))
            {
                NavView.SelectedItem = VertragListMenuItem;
            }
            else if (e.SourcePageType == typeof(BetriebskostenrechnungListViewPage) || e.SourcePageType == typeof(BetriebskostenrechnungDetailViewPage))
            {
                NavView.SelectedItem = BetriebskostenrechnungListMenuItem;
            }
            else if (e.SourcePageType == typeof(ErhaltungsaufwendungListViewPage) || e.SourcePageType == typeof(ErhaltungsaufwendungDetailViewPage))
            {
                NavView.SelectedItem = ErhaltungsaufwendungListMenuItem;
            }
            else if (e.SourcePageType == typeof(ZaehlerListViewPage) || e.SourcePageType == typeof(ZaehlerDetailViewPage))
            {
                NavView.SelectedItem = ZaehlerListMenuItem;
            }
            else if (e.SourcePageType == typeof(UmlageListViewPage) || e.SourcePageType == typeof(UmlageDetailViewPage))
            {
                NavView.SelectedItem = UmlageListMenuItem;
            }
            else if (e.SourcePageType == typeof(SettingsViewPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            if (e.NavigationMode == NavigationMode.Back)
            {
                Navigate(e.SourcePageType, (NavView.SelectedItem as NavigationViewItem).Content);
            }
        }

        private async Task<bool> checkOutOfSync()
        {
            var ns = App.Container.GetInstance<INotificationService>();
            if (ns.outOfSync)
            {
                if (!await ns.Confirmation(
                    "Sind Sie sicher?",
                    "Sie haben ungespeicherte Änderungen.",
                    "Ja", "Nein"))
                {
                    return true;
                }
                else
                {
                    ns.outOfSync = false;
                }
            }

            return false;
        }

        private async void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (await checkOutOfSync())
            {
                return;
            }

            if (AppFrame.CanGoBack && !App.Container.GetInstance<INotificationService>().outOfSync)
            {
                AppFrame.GoBack();
            }
        }

        private Type GetEntryPage(object Entry)
        {
            return Entry is NatuerlichePerson ? typeof(NatuerlichePersonDetailViewPage) :
                Entry is JuristischePerson ? typeof(JuristischePersonDetailViewPage) :
                Entry is Wohnung ? typeof(WohnungDetailViewPage) :
                Entry is Zaehler ? typeof(ZaehlerDetailViewPage) :
                Entry is Vertrag ? typeof(VertragDetailViewPage) :
                Entry is Betriebskostenrechnung ? typeof(BetriebskostenrechnungDetailViewPage) :
                Entry is Erhaltungsaufwendung ? typeof(ErhaltungsaufwendungDetailViewPage) :
                Entry is Umlage ? typeof(UmlageDetailViewPage) :
                null;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            AutoSuggest.update(sender.Text);
            sender.IsSuggestionListOpen = true;
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion == null)
            {
                return;
            }
            var Entity = (args.ChosenSuggestion as AutoSuggestListViewModelEntry).Entity;
            Navigate(GetEntryPage(Entity), Entity);
            sender.IsSuggestionListOpen = false;
            sender.Text = "";
        }
    }
}
