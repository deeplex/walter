using Deeplex.Saverwalter.App.Views;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public Frame AppFrame => frame;

        public readonly string KontaktListLabel = "Kontakte";
        public readonly string VertragListLabel = "Verträge";
        public readonly string WohnungListLabel = "Mietobjekte";
        public readonly string JuristischePersonenListLabel = "Jur. Personen";
        public readonly string BetriebskostenrechnungenListLabel = "Betr. Rechnung";

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var label = args.InvokedItem as string;
            var pageType =
                args.IsSettingsInvoked ? typeof(SettingsPage) :
                label == KontaktListLabel ? typeof(KontaktListPage) :
                label == WohnungListLabel ? typeof(WohnungListPage) :
                label == VertragListLabel ? typeof(VertragListPage) :
                label == JuristischePersonenListLabel ? typeof(JuristischePersonenListPage) :
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
    }
}
