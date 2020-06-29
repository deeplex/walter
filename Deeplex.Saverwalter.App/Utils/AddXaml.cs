using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Deeplex.Saverwalter.App.Utils
{
    public static class AddXaml
    {
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
                AllowFocusOnInteraction = true,
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
                Width = 150,
                AllowFocusOnInteraction = true,
                Header = "Kostentyp",
                PlaceholderText = "Wähle Typen aus…",
                ItemsSource = ViewModel.Betriebskostentypen,
                DisplayMemberPath = "Beschreibung",
            };

            Panel2.Children.Add(Kostentyp);
            var UmlageSchluessel = new ComboBox
            {
                AllowFocusOnInteraction = true,
                Header = "Umlageschlüssel",
                Margin = new Thickness(10, 0, 0, 0),
                SelectedIndex = 0, // n.WF
                ItemsSource = ViewModel.Betriebskostenschluessel,
                DisplayMemberPath = "Beschreibung"
            };
            Panel2.Children.Add(UmlageSchluessel);
            var BetreffendesJahr = new NumberBox
            {
                AllowFocusOnInteraction = true,
                Header = "Betr. Jahr",
                Value = DateTime.UtcNow.Year - 1,
                Margin = new Thickness(10, 0, 0, 0),
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            };
            Panel2.Children.Add(BetreffendesJahr);
            Panel.Children.Add(Panel2);

            var Panel2_5 = new StackPanel
            {
                Visibility = (Visibility)(isHeizkost(Kostentyp.SelectedValue) ? 0 : 1),
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0),
            };
            Kostentyp.SelectionChanged += Kostentyp_SelectionChanged;

            void Kostentyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                Panel2_5.Visibility = (Visibility)(isHeizkost(Kostentyp.SelectedValue) ? 0 : 1);
            }

            bool isHeizkost(object a)
                => ((BetriebskostenRechnungenBetriebskostenTyp)a)?.Typ == Betriebskostentyp.Heizkosten;

            var Para7 = new Slider
            {
                Header = "HKVO §7",
                Width = 100,
                Minimum = 50,
                Maximum = 70,
            };
            var Para7T = new TextBlock
            {
                Width = 18,
                Margin = new Thickness(10, 0, 0, 8),
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = Para7.Value.ToString(),
            };

            Para7.ValueChanged += Update7;

            void Update7(object sender, RangeBaseValueChangedEventArgs e)
            {
                Para7T.Text = Para7.Value.ToString();
            }

            Panel2_5.Children.Add(Para7);
            Panel2_5.Children.Add(Para7T);
            var Para8 = new Slider
            {
                Header = "HKVO §8",
                Margin = new Thickness(10, 0, 0, 0),
                Width = 100,
                Minimum = 50,
                Maximum = 70,
            };
            var Para8T = new TextBlock
            {
                Width = 18,
                Margin = new Thickness(10, 0, 0, 8),
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = Para8.Value.ToString(),
            };

            Para8.ValueChanged += Update8;

            void Update8(object sender, RangeBaseValueChangedEventArgs e)
            {
                Para8T.Text = Para8.Value.ToString();
            }

            Panel2_5.Children.Add(Para8);
            Panel2_5.Children.Add(Para8T);

            var Para9 = new ComboBox
            {
                AllowFocusOnInteraction = true,
                Header = "HKVO §9",
                ItemsSource = ViewModel.HKVO9,
                DisplayMemberPath = "Absatz",
                SelectedIndex = 1, // Absatz 2
                Margin = new Thickness(10, 0, 0, 0),
            };
            Panel2_5.Children.Add(Para9);

            Panel.Children.Add(Panel2_5);

            var Panel3 = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0),
            };
            var Datum = new CalendarDatePicker { Header = "Datum", };
            Panel3.Children.Add(Datum);
            var Betrag = new NumberBox
            {
                AllowFocusOnInteraction = true,
                Width = 106, // Defined by Header of Umlageschlüssel
                Header = "Betrag",
                Margin = new Thickness(10, 0, 0, 0),
            };
            Panel3.Children.Add(Betrag);
            Panel3.Children.Add(new Button
            {
                IsEnabled = false, // TODO
                Margin = new Thickness(10, 20, 0, 0),
                Content = new SymbolIcon(Symbol.Attach),
            });
            var AddButton = new Button
            {
                Content = new SymbolIcon(Symbol.Add),
                Margin = new Thickness(10, 20, 0, 0),
            };

            AddButton.Click += AddBetriebskostenrechnung_Click;
            Panel3.Children.Add(AddButton);

            Panel.Children.Add(Panel3);

            return new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Margin = new Thickness(10, 0, 0, 0),
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
                if (isHeizkost(Kostentyp.SelectedValue))
                {
                    r.HKVO_P7 = Para7.Value / 100;
                    r.HKVO_P8 = Para8.Value / 100;
                    r.HKVO_P9 = (HKVO_P9A2)Para9.SelectedValue;
                }
                App.Walter.Betriebskostenrechnungen.Add(r);

                Tree.SelectedItems
                    .Where(s => s is BetriebskostenRechungenListWohnungListWohnung)
                    .ToList().ForEach(b =>
                    {
                        App.Walter.Betriebskostenrechnungsgruppen.Add(new BetriebskostenrechnungsGruppe
                        {
                            WohnungId = ((BetriebskostenRechungenListWohnungListWohnung)b).Id,
                            Rechnung = r
                        });
                    });

                App.Walter.SaveChanges();
            }
        }

        private static void Kostentyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
