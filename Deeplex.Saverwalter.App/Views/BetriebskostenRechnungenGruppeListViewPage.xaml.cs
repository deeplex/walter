using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{

    public sealed partial class BetriebskostenRechnungenGruppeListViewPage : Page
    {
        public BetriebskostenRechnungenGruppeListViewModel ViewModel = new BetriebskostenRechnungenGruppeListViewModel();
        public BetriebskostenRechnungenGruppeListViewPage()
        {
            InitializeComponent();
        }

        private void SortiereNachTyp_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenRechnungenTypListViewPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
