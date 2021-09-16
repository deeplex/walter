using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class BetriebskostenRechnungenListViewPage : Page
    {
        public BetriebskostenRechnungenListViewModel ViewModel = new BetriebskostenRechnungenListViewModel(App.ViewModel);

        public BetriebskostenRechnungenListViewPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Betriebskostenrechnungen";

            var AddBetriebskostenRechnung = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Betriebskostenrechnung hinzufügen",
            };
            AddBetriebskostenRechnung.Click += AddBetriebskostenWohnung_Click;

            App.Window.RefillCommandContainer(new ICommandBarElement[]
            {
                Elements.Filter(ViewModel),
                AddBetriebskostenRechnung,
            });
        }

        private void AddBetriebskostenWohnung_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenrechnungenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
