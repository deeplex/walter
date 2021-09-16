using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class ZaehlerListPage : Page
    {
        public ZaehlerListViewModel ViewModel = new ZaehlerListViewModel(App.ViewModel);

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
