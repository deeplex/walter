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

    public sealed partial class BetriebskostenRechnungenTypListViewPage : Page
    {
        public BetriebskostenRechnungenListViewModel ViewModel = new BetriebskostenRechnungenListViewModel();
        public BetriebskostenRechnungenTypListViewPage()
        {
            InitializeComponent();

            App.ViewModel.Titel.Value = "Betriebskostenrechnungen";
            var Sortiere = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Sync),
                Label = "Sortiere nach Gruppe"
            };
            Sortiere.Click += SortiereNachGruppen_Click;

            var Add = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Hinzufügen"
            };
            Add.Click += Add_Click;

            var EditToggle = new AppBarToggleButton
            {
                Label = "Bearbeiten",
                Icon = new SymbolIcon(Symbol.Edit),
            };
            EditToggle.Click += EditToggle_Click;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                Sortiere, Add,
            }, new ICommandBarElement[] { EditToggle });
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Navigate(typeof(BetriebskostenrechnungenDetailPage), null);
        }

        private void SortiereNachGruppen_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenRechnungenGruppeListViewPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void NeuesJahr_Click(object sender, RoutedEventArgs e)
        {
            var dc = (KeyValuePair<BetriebskostenRechnungenBetriebskostenTyp, BetriebskostenRechnungenListJahr>)((Button)sender).DataContext;
            var LetztesJahr = dc.Value.Jahre.Value.First();
            var Rechnungen = LetztesJahr.Value;

            var neueJahre = new BetriebskostenRechnungenListJahr(
                ViewModel,
                ViewModel.Typen.Value[dc.Key].Jahre.Value.Add(
                LetztesJahr.Key + 1,
                Rechnungen.Select(r => new BetriebskostenRechnungenRechnung(ViewModel, r)).ToImmutableList()));

            var Typen = ViewModel.Typen.Value.Remove(dc.Key);
            ViewModel.Typen.Value = Typen.Add(dc.Key, neueJahre);
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
            App.SaveWalter();
            cp.isNew.Value = false;
        }

        private void EditToggle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsInEdit.Value = (sender as AppBarToggleButton).IsChecked ?? false;
        }
    }
}
