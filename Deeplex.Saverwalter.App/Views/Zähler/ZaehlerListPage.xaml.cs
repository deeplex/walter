using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.ViewModels.Zähler;
using Deeplex.Saverwalter.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class ZaehlerListPage : Page
    {
        public ZaehlerListViewModel ViewModel = new ZaehlerListViewModel();

        public ZaehlerListPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Zähler";
            var AddZaehler = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Zähler hinzufügen",
            };
            AddZaehler.Click += AddZaehler_Click;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[] { Elements.Filter(ViewModel), AddZaehler });
        }

        private void AddZaehler_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ZaehlerDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
