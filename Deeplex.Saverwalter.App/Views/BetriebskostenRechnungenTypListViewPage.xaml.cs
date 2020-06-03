using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Deeplex.Saverwalter.App.Views
{

    public sealed partial class BetriebskostenRechnungenTypListViewPage : Page
    {
        public BetriebskostenRechnungenTypListViewModel ViewModel = new BetriebskostenRechnungenTypListViewModel();
        public BetriebskostenRechnungenTypListViewPage()
        {
            InitializeComponent();

            ViewModel.AdresseGroup.Keys.ToList().ForEach(k =>
            {
                ViewModel.AdresseGroup[k].ForEach(v => k.Children.Add(v));
                AddBetriebskostenrechnungBetroffeneWohneinheiten.RootNodes.Add(k);
            });
        }

        private void AddBetriebskostenrechnung_Click(object sender, RoutedEventArgs e)
        {
            var r = new Betriebskostenrechnung()
            {
                Beschreibung = AddBetriebskostenrechnungBeschreibung.Text,
                Datum = AddBetriebskostenrechnungDatum.Date.Value.UtcDateTime,
                Schluessel = ((BetriebskostenRechnungenSchluessel)AddBetriebskostenrechnungSchluessel.SelectedItem).Schluessel,
                Typ = ((BetriebskostenRechnungenBetriebskostentyp)AddBetriebskostenrechnungTyp.SelectedItem).Typ,
                Betrag = AddBetriebskostenrechnungBetrag.Value,
            };
            App.Walter.Betriebskostenrechnungen.Add(r);

            AddBetriebskostenrechnungBetroffeneWohneinheiten.SelectedItems
                .Where(s => s is BetriebskostenRechungenListWohnungListWohnung)
                .ToList()
                .ForEach(b =>
                {
                    App.Walter.Betriebskostenrechnungsgruppen.Add(new Betriebskostenrechnungsgruppe
                    {
                        WohnungId = ((BetriebskostenRechungenListWohnungListWohnung)b).Id,
                        Rechnung = r
                    });
                });

            App.Walter.SaveChanges();
        }
    }
}
