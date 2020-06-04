using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using static Deeplex.Saverwalter.App.ViewModels.BetriebskostenRechnungenGruppeListViewModel;

namespace Deeplex.Saverwalter.App.Views
{

    public sealed partial class BetriebskostenRechnungenGruppeListViewPage : Page
    {
        public BetriebskostenRechnungenGruppeListViewModel ViewModel = new BetriebskostenRechnungenGruppeListViewModel();
        public BetriebskostenRechnungenGruppeListViewPage()
        {
            InitializeComponent();

            ViewModel.AdresseGroup.Keys.ToList().ForEach(k =>
            {
                ViewModel.AdresseGroup[k].ForEach(v => k.Children.Add(v));
                AddBetriebskostenrechnungBetroffeneWohneinheiten.RootNodes.Add(k);
            });
            AddBetriebskostenrechnungSchluessel.SelectedIndex = 0; // n.WF.
            AddBetriebskostenrechnungJahr.Value = DateTime.UtcNow.Year - 1;
        }

        private void AddBetriebskostenrechnung_Click(object sender, RoutedEventArgs e)
        {
            var r = new Betriebskostenrechnung()
            {
                Beschreibung = AddBetriebskostenrechnungBeschreibung.Text,
                Datum = AddBetriebskostenrechnungDatum.Date.Value.UtcDateTime,
                Schluessel = ((BetriebskostenRechnungenSchluessel)AddBetriebskostenrechnungSchluessel.SelectedItem).Schluessel,
                Typ = ((BetriebskostenRechnungenBetriebskostentyp)AddBetriebskostenrechnungTyp.SelectedItem).Typ,
                BetreffendesJahr = (int)AddBetriebskostenrechnungJahr.Value,
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

        private void SortiereNachTyp_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenRechnungenTypListViewPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void NeuesJahr_Click(object sender, RoutedEventArgs e)
        {
            var dc = (KeyValuePair<BetriebskostenRechnungenGruppeListGruppe, BetriebskostenRechnungenListGruppenJahr>)((Button)sender).DataContext;
            var LetztesJahr = dc.Value.Jahre.First();
            var Rechnungen = LetztesJahr.Value;

            var neueJahre = new BetriebskostenRechnungenListGruppenJahr(
                ViewModel.Gruppen.Value[dc.Key].Jahre.Add(
                LetztesJahr.Key + 1,
                Rechnungen.Select(r => new BetriebskostenGruppenRechnungenRechnung(r)).ToImmutableList()));

            var Typen = ViewModel.Gruppen.Value.Remove(dc.Key);
            ViewModel.Gruppen.Value = Typen.Add(dc.Key, neueJahre);
        }

        private void SaveGruppe_Click(object sender, RoutedEventArgs e)
        {
            var cp = (BetriebskostenGruppenRechnungenRechnung)((Button)sender).CommandParameter;
            var r = new Betriebskostenrechnung
            {
                Betrag = cp.Betrag,
                Beschreibung = cp.Beschreibung,
                BetreffendesJahr = cp.BetreffendesJahr,
                Datum = cp.Datum.UtcDateTime.AsUtcKind(),
                Notiz = cp.Notiz,
                Schluessel = cp.Schluessel,
                Typ = cp.Typ
            };
            App.Walter.Betriebskostenrechnungen.Add(r);
            foreach (var w in cp.WohnungenIds)
            {
                App.Walter.Betriebskostenrechnungsgruppen.Add(new Betriebskostenrechnungsgruppe
                {
                    Rechnung = r,
                    WohnungId = w
                });

            }
            App.Walter.SaveChanges();
            cp.AddEntity(r);
        }
    }
}
