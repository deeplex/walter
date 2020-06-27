using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{

    public sealed partial class BetriebskostenRechnungenTypListViewPage : Page
    {
        public BetriebskostenRechnungenListViewModel ViewModel = new BetriebskostenRechnungenListViewModel();
        public BetriebskostenRechnungenTypListViewPage()
        {
            InitializeComponent();

            ViewModel.AdresseGroup.Keys.ToList().ForEach(k =>
            {
                ViewModel.AdresseGroup[k].ForEach(v => k.Children.Add(v));
                AddBetriebskostenrechnungBetroffeneWohneinheiten.RootNodes.Add(k);
            });
            AddBetriebskostenrechnungSchluessel.SelectedIndex = 0; // n.WF.
            AddBetriebskostenrechnungJahr.Value = DateTime.UtcNow.Year - 1;

            App.ViewModel.Titel.Value = "Betriebskostenrechnungen";
        }

        private void AddBetriebskostenrechnung_Click(object sender, RoutedEventArgs e)
        {
            var r = new Betriebskostenrechnung()
            {
                Beschreibung = AddBetriebskostenrechnungBeschreibung.Text,
                Datum = AddBetriebskostenrechnungDatum.Date.Value.UtcDateTime,
                Schluessel = ((BetriebskostenRechnungenSchluessel)AddBetriebskostenrechnungSchluessel.SelectedItem).Schluessel,
                Typ = ((BetriebskostenRechnungenBetriebskostenTyp)AddBetriebskostenrechnungTyp.SelectedItem).Typ,
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

        private void SortiereNachGruppen_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenRechnungenGruppeListViewPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void NeuesJahr_Click(object sender, RoutedEventArgs e)
        {
            var dc = (KeyValuePair<BetriebskostenRechnungenBetriebskostenTyp, BetriebskostenRechnungenListJahr>)((Button)sender).DataContext;
            var LetztesJahr = dc.Value.Jahre.First();
            var Rechnungen = LetztesJahr.Value;

            var neueJahre = new BetriebskostenRechnungenListJahr(
                ViewModel.Typen.Value[dc.Key].Jahre.Add(
                LetztesJahr.Key + 1,
                Rechnungen.Select(r => new BetriebskostenRechnungenRechnung(r)).ToImmutableList()));

            var Typen = ViewModel.Typen.Value.Remove(dc.Key);
            ViewModel.Typen.Value = Typen.Add(dc.Key, neueJahre);
        }

        private void SaveGruppe_Click(object sender, RoutedEventArgs e)
        {
            var cp = (BetriebskostenRechnungenRechnung)((Button)sender).CommandParameter;
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
