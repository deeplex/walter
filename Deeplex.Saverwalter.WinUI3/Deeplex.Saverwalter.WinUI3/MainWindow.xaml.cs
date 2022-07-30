using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
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

            if (SendParameter is IAnhang a)
            {
                DetailAnhang.Value = new AnhangListViewModel(a, App.FileService, App.NotificationService, App.WalterService);
            }
            else
            {
                DetailAnhang.Value = null;
            }
            ListAnhang.Value = null;

            AppFrame.Navigate(SourcePage, SendParameter,
                new DrillInNavigationTransitionInfo());
        }

        public MainWindow()
        {
            InitializeComponent();

            MainGrid.Loaded += Root_Loaded;
        }

        private async Task initializeDatabase()
        {
            if (App.WalterService.root == null || !File.Exists(App.WalterService.root + ".db"))
            {
                var path = await App.NotificationService.Confirmation(
                        "Noch keine Datenbank ausgewählt",
                        "Datenbank suchen, oder leere Datenbank erstellen?",
                        "Existierende Datenbank auswählen", "Erstelle neue leere Datenbank") ?
                        await App.FileService.pickFile(".db") :
                        await App.FileService.saveFile("walter", new string[] { ".db" });
                App.WalterService.root = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            }

            if (App.WalterService.ctx != null)
            {
                App.WalterService.ctx.Dispose();
            }
            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseSqlite("Data Source=" + App.WalterService.root + ".db");
            App.WalterService.ctx = new SaverwalterContext(optionsBuilder.Options);
            App.WalterService.ctx.Database.Migrate();

        }

        private async void Root_Loaded(object sender, RoutedEventArgs e)
        {
            var Settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var str = Settings.Values["root"] as string;
            App.WalterService.root = str;
            await initializeDatabase();

            if (str != App.WalterService.root)
            {
                Utils.Elements.SetDatabaseAsDefault();
            }

            AutoSuggest = new AutoSuggestListViewModel(App.WalterService);
        }

        public Frame AppFrame => frame;

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
                args.IsSettingsInvoked ? typeof(SettingsViewPage) :
                label == KontaktListLabel ? typeof(KontaktListViewPage) :
                label == WohnungListLabel ? typeof(WohnungListViewPage) :
                label == VertragListLabel ? typeof(VertragListViewPage) :
                label == ZaehlerListLabel ? typeof(ZaehlerListViewPage) :
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
            if (e.SourcePageType == typeof(KontaktListViewPage) ||
                e.SourcePageType == typeof(JuristischePersonenDetailViewPage) ||
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
            else if (e.SourcePageType == typeof(BetriebskostenRechnungenListViewPage) || e.SourcePageType == typeof(BetriebskostenrechnungenDetailViewPage))
            {
                NavView.SelectedItem = BetriebskostenListMenuItem;
            }
            else if (e.SourcePageType == typeof(ErhaltungsaufwendungenListViewPage) || e.SourcePageType == typeof(ErhaltungsaufwendungenDetailViewPage))
            {
                NavView.SelectedItem = ErhaltungsAufwendungenListMenuItem;
            }
            else if (e.SourcePageType == typeof(ZaehlerListViewPage) || e.SourcePageType == typeof(ZaehlerDetailViewPage))
            {
                NavView.SelectedItem = ZaehlerListMenuItem;
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
            if (App.NotificationService.outOfSync)
            {
                if (!await App.NotificationService.Confirmation(
                    "Sind Sie sicher?",
                    "Sie haben ungespeicherte Änderungen.",
                    "Ja", "Nein"))
                {
                    return true;
                }
                else
                {
                    App.NotificationService.outOfSync = false;
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

            if (AppFrame.CanGoBack && !App.NotificationService.outOfSync)
            {
                AppFrame.GoBack();
            }
        }

        private Type GetEntryPage(object Entry)
        {
            return Entry is NatuerlichePerson ? typeof(NatuerlichePersonDetailViewPage) :
                Entry is JuristischePerson ? typeof(JuristischePersonenDetailViewPage) :
                Entry is Wohnung ? typeof(WohnungDetailViewPage) :
                Entry is Zaehler ? typeof(ZaehlerDetailViewPage) :
                Entry is Vertrag ? typeof(VertragDetailViewPage) :
                Entry is Betriebskostenrechnung ? typeof(BetriebskostenrechnungenDetailViewPage) :
                Entry is Erhaltungsaufwendung ? typeof(ErhaltungsaufwendungenDetailViewPage) :
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
