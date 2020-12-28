using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{

    public sealed partial class BetriebskostenRechnungenGruppeListViewPage : Page
    {
        public BetriebskostenRechnungenListViewModel ViewModel = new BetriebskostenRechnungenListViewModel();

        public BetriebskostenRechnungenGruppeListViewPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Betriebskostenrechnungen";

            var Sortiere = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Sync),
                Label = "Sortiere nach Typ"
            };
            Sortiere.Click += SortiereNachTyp_Click;

            var AddBetriebskostenRechnung = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Betriebskostenrechnung hinzufügen",
            };
            AddBetriebskostenRechnung.Click += AddBetriebskostenWohnung_Click;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                Sortiere,
                AddBetriebskostenRechnung,
            });
        }

        private void AddBetriebskostenWohnung_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenrechnungenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void SortiereNachTyp_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenRechnungenTypListViewPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void NeuesJahr_Click(object sender, RoutedEventArgs e)
        {
            //var dc = (KeyValuePair<BetriebskostenRechnungenBetriebskostenGruppe, BetriebskostenRechnungenListJahr>)((Button)sender).DataContext;
            //var LetztesJahr = dc.Value.Jahre.Value.First();
            //var Rechnungen = LetztesJahr.Value;
            //var neueJahre = new BetriebskostenRechnungenListJahr(
            //    ViewModel,
            //    ViewModel.Gruppen.Value[dc.Key].Jahre.Value.Add(
            //    LetztesJahr.Key + 1,
            //    Rechnungen.Select(r => new BetriebskostenRechnungenRechnung(ViewModel, r)).ToImmutableList()));
            //var Gruppen = ViewModel.Gruppen.Value.Remove(dc.Key);
            //ViewModel.Gruppen.Value = Gruppen.Add(dc.Key, neueJahre);
        }

        private void SaveGruppe_Click(object sender, RoutedEventArgs e)
        {
            //var cp = (BetriebskostenRechnungenRechnung)((Button)sender).CommandParameter;
            //var r = cp.GetEntity;
            //if (r != null)
            //{
            //    App.Walter.Betriebskostenrechnungen.Update(r);
            //}
            //else
            //{
            //    App.Walter.Betriebskostenrechnungen.Add(r);
            //}
            //foreach (var w in cp.WohnungenIds)
            //{
            //    App.Walter.Betriebskostenrechnungsgruppen.Add(new BetriebskostenrechnungsGruppe
            //    {
            //        Rechnung = r,
            //        WohnungId = w
            //    });
            //}
            //App.SaveWalter();
            //cp.isNew.Value = false;
        }
    }
}
