using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.UI.Xaml.Controls;
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

            App.ViewModel.Titel.Value = "Betriebskostenrechnungen";
            var Sortiere = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Sync),
                Label = "Sortiere nach Gruppe"
            };
            Sortiere.Click += SortiereNachGruppen_Click;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                Sortiere, AddBetriebskostenrechnung(ViewModel),
            });
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

        public static AppBarButton AddBetriebskostenrechnung(BetriebskostenRechnungenListViewModel ViewModel)
        {
            var Panel = new StackPanel();
            Panel.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 20),
                Style = (Style)App.Current.Resources["SubtitleTextBlockStyle"],
                Text = "Füge Betriebskostenrechnung hinzu:"
            });
            Panel.Children.Add(new TextBlock
            {
                Text = "Betroffene Wohneinheiten",
            });
            var Tree = new Microsoft.UI.Xaml.Controls.TreeView
            {
                SelectionMode = Microsoft.UI.Xaml.Controls.TreeViewSelectionMode.Multiple,
            };
            ViewModel.AdresseGroup.Keys.ToList().ForEach(k =>
            {
                ViewModel.AdresseGroup[k].ForEach(v => k.Children.Add(v));
                Tree.RootNodes.Add(k);
            });
            Panel.Children.Add(Tree);
            var Beschreibung = new TextBox
            {
                Header = "Beschreibung",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Panel.Children.Add(Beschreibung);
            var Panel2 = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0),
            };
            var Kostentyp = new ComboBox
            {
                Header = "Kostentyp",
                ItemsSource = ViewModel.Betriebskostentypen,
                DisplayMemberPath = "Beschreibung",
            };
            Panel2.Children.Add(Kostentyp);
            var UmlageSchluessel = new ComboBox
            {
                Header = "Umlageschlüssel",
                SelectedIndex = 0, // n.WF
                ItemsSource = ViewModel.Betriebskostenschluessel,
                DisplayMemberPath = "Beschreibung"
            };
            Panel2.Children.Add(UmlageSchluessel);
            var BetreffendesJahr = new NumberBox
            {
                Header = "Betr. Jahr",
                Value = DateTime.UtcNow.Year - 1,
                Margin = new Thickness(10, 0, 0, 0),
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            };
            Panel2.Children.Add(BetreffendesJahr);
            Panel.Children.Add(Panel2);

            var Panel3 = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0),
            };
            var Datum = new CalendarDatePicker { Header = "Datum", };
            Panel3.Children.Add(Datum);
            var Betrag = new NumberBox
            {
                Header = "Betrag",
                Margin = new Thickness(10, 0, 0, 0),
            };
            Panel3.Children.Add(Betrag);
            Panel3.Children.Add(new Button
            {
                IsEnabled = false, // TODO
                Margin = new Thickness(10, 0, 0, 0),
                Content = new SymbolIcon(Symbol.Attach),
            });
            var AddButton = new Button { Content = new SymbolIcon(Symbol.Add), };


            AddButton.Click += AddBetriebskostenrechnung_Click;
            Panel3.Children.Add(AddButton);

            Panel.Children.Add(Panel3);

            return new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Hinzufügen",
                Flyout = new Flyout { Content = Panel }
            };

            void AddBetriebskostenrechnung_Click(object sender, RoutedEventArgs e)
            {
                var r = new Betriebskostenrechnung()
                {
                    Beschreibung = Beschreibung.Text,
                    Datum = Datum.Date.Value.UtcDateTime,
                    Schluessel = ((BetriebskostenRechnungenSchluessel)UmlageSchluessel.SelectedItem).Schluessel,
                    Typ = ((BetriebskostenRechnungenBetriebskostenTyp)Kostentyp.SelectedItem).Typ,
                    BetreffendesJahr = (int)BetreffendesJahr.Value,
                    Betrag = Betrag.Value,
                };
                App.Walter.Betriebskostenrechnungen.Add(r);

                Tree.SelectedItems
                    .Where(s => s is BetriebskostenRechungenListWohnungListWohnung)
                    .ToList().ForEach(b =>
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
}
