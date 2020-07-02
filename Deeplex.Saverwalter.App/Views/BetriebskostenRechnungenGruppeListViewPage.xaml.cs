﻿using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

            var EditToggle = new AppBarToggleButton
            {
                Label = "Bearbeiten",
                Icon = new SymbolIcon(Symbol.Edit),
            };
            EditToggle.Click += EditToggle_Click;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                Sortiere, AddXaml.AddBetriebskostenrechnung(ViewModel),
            }, new ICommandBarElement[] { EditToggle });
        }

        private void SortiereNachTyp_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenRechnungenTypListViewPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void NeuesJahr_Click(object sender, RoutedEventArgs e)
        {
            var dc = (KeyValuePair<BetriebskostenRechnungenBetriebskostenGruppe, BetriebskostenRechnungenListJahr>)((Button)sender).DataContext;
            var LetztesJahr = dc.Value.Jahre.First();
            var Rechnungen = LetztesJahr.Value;

            var neueJahre = new BetriebskostenRechnungenListJahr(
                ViewModel,
                ViewModel.Gruppen.Value[dc.Key].Jahre.Add(
                    LetztesJahr.Key + 1,
                    Rechnungen.Select(r => new BetriebskostenRechnungenRechnung(ViewModel, r)).ToImmutableList()));

            var Typen = ViewModel.Gruppen.Value.Remove(dc.Key);
            ViewModel.Gruppen.Value = Typen.Add(dc.Key, neueJahre);
        }

        private void SaveGruppe_Click(object sender, RoutedEventArgs e)
        {
            var cp = (BetriebskostenRechnungenRechnung)((Button)sender).CommandParameter;
            var r = cp.GetEntity;
            if (r != null)
            {
                App.Walter.Betriebskostenrechnungen.Update(r);
            }
            else
            {
                App.Walter.Betriebskostenrechnungen.Add(r);
            }
            foreach (var w in cp.WohnungenIds)
            {
                App.Walter.Betriebskostenrechnungsgruppen.Add(new BetriebskostenrechnungsGruppe
                {
                    Rechnung = r,
                    WohnungId = w
                });
            }
            App.Walter.SaveChanges();
            cp.isNew.Value = false;
        }

        private void EditToggle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsInEdit.Value = (sender as AppBarToggleButton).IsChecked ?? false;
        }
    }
}
