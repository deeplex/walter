﻿using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Deeplex.Saverwalter.App.Views
{

    public sealed partial class BetriebskostenRechnungenListViewPage : Page
    {
        public BetriebskostenRechnungenListViewModel ViewModel = new BetriebskostenRechnungenListViewModel();
        public BetriebskostenRechnungenListViewPage()
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
                .ForEach(b => {
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