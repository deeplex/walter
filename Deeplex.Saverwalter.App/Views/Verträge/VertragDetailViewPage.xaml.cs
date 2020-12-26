using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Print;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class VertragDetailViewPage : Page
    {
        public VertragDetailViewModel ViewModel { get; set; }

        public VertragDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Guid vertragId)
            {
                ViewModel = new VertragDetailViewModel(vertragId);
            }
            else // If invoked using "Add"
            {
                ViewModel = new VertragDetailViewModel();
            }

            // TODO this can be fixed by having the selected item be a pointer to the respecitve element in the list.
            if (ViewModel.Wohnung != null)
            {
                WohnungComboBox.SelectedIndex = ViewModel.AlleWohnungen.FindIndex(w => w.Id == ViewModel.Wohnung.Id);
            }
            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            if (ViewModel.Ansprechpartner != null) // TODO this is an ugly way to accomplish initial loading of Besitzer in GUI
            {
                AnsprechpartnerSuggest.Text = ViewModel.Ansprechpartner.Bezeichnung;
            }
            FillMieterBox();
            base.OnNavigatedTo(e);
            App.ViewModel.Titel.Value = "Vertragdetails";

            var Primary = new ICommandBarElement[] {
                BetriebskostenAbrechnungsButton(),
                new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Attach),
                    Command = ViewModel.AttachFile,
                }
            };

            var EditToggle = new AppBarToggleButton
            {
                Label = "Bearbeiten",
                Icon = new SymbolIcon(Symbol.Edit),
            };
            EditToggle.Click += EditToggle_Click;

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
                IsEnabled = false, // TODO
            };

            var Secondary = new ICommandBarElement[]
            {
                EditToggle, Delete,
            };

            App.ViewModel.RefillCommandContainer(Primary, Secondary);
        }

        private AppBarButton BetriebskostenAbrechnungsButton()
        {
            var BetrKostenAbrButtons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            BetrKostenAbrButtons.Children.Add(new NumberBox()
            {
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                AllowFocusOnInteraction = true,
                Value = ViewModel.BetriebskostenJahr.Value,
            });
            var AddBetrKostenAbrBtn = new Button
            {
                CommandParameter = ViewModel.BetriebskostenJahr.Value,
                Content = new SymbolIcon(Symbol.SaveLocal),
            };
            BetrKostenAbrButtons.Children.Add(AddBetrKostenAbrBtn);
            AddBetrKostenAbrBtn.Click += Betriebskostenabrechnung_Click;
            return new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.PostUpdate),
                Label = "Betriebskostenabrechnung",
                Flyout = new Flyout()
                {
                    Content = BetrKostenAbrButtons,
                },
            };
        }

        private void FillMieterBox()
        {
            foreach (var mieter in ViewModel.Mieter.Value)
            {
                MieterBox.AddTokenItem(mieter);
            }
        }

        private void AnsprechpartnerSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sender.ItemsSource = ViewModel.AlleKontakte
                    // If Checkbox => .Where(k => k.JuristischePersonen.Contains(ViewModel.Vermieter.Id))
                    .Where(k => k.Bezeichnung.Trim().ToLower().Contains(sender.Text.Trim().ToLower()))
                    .ToImmutableList();
        }

        private void AnsprechpartnerSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is VertragDetailKontakt a)
            {
                var ansprechpartner = ViewModel.AlleKontakte.First(k => k.PersonId == a.PersonId);

                ViewModel.Ansprechpartner = ansprechpartner;
                ViewModel.Versionen.Value.ForEach(v => v.Ansprechpartner = ansprechpartner);
                App.Walter.Vertraege.Where(vs => vs.VertragId == ViewModel.guid).ToList().ForEach(vs =>
                {
                    vs.AnsprechpartnerId = a.PersonId;
                });
                App.SaveWalter();
            }
        }

        private void RemoveDate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Ende = null;
            ViewModel.Versionen.Value.Last().Ende = null;
        }

        private void RemoveMiete_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var miete = (VertragDetailMiete)((Button)sender).CommandParameter;
            ViewModel.Mieten.Value = ViewModel.Mieten.Value.Remove(miete).ToImmutableList();
            miete.selfDestruct();
        }

        private void RemoveMietMinderung_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var mietminderung = (VertragDetailMietMinderung)((Button)sender).CommandParameter;
            ViewModel.MietMinderungen.Value = ViewModel.MietMinderungen.Value.Remove(mietminderung).ToImmutableList();
            mietminderung.selfDestruct();
        }

        private void Betriebskostenabrechnung_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var Jahr = (int)((Button)sender).CommandParameter;
            var b = new Betriebskostenabrechnung(
                App.Walter,
                ViewModel.Versionen.Value.First().Id,
                Jahr,
                new DateTime(Jahr, 1, 1),
                new DateTime(Jahr, 12, 31));

            var s = Jahr.ToString() + " - " + ViewModel.Wohnung.BezeichnungVoll;

            b.SaveAsDocx(ApplicationData.Current.LocalFolder.Path + @"\" + s + ".docx");
        }

        private void EditToggle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsInEdit.Value = (sender as AppBarToggleButton).IsChecked ?? false;
        }

        private void MieterBox_TokenItemRemoved(Microsoft.Toolkit.Uwp.UI.Controls.TokenizingTextBox sender, object args)
        {
            if (args is VertragDetailKontakt mieter)
            {
                ViewModel.Mieter.Value = ViewModel.Mieter.Value.Remove(mieter);
                ViewModel.AlleMieter.Value = ViewModel.AlleMieter.Value.Add(mieter);
                var target = App.Walter.MieterSet.FirstOrDefault(m =>
                    m.VertragId == ViewModel.guid && m.PersonId == mieter.PersonId);
                if (target != null)
                {
                    App.Walter.MieterSet.Remove(target);
                    App.SaveWalter();
                }
            }
        }
        private void MieterBox_TokenItemAdded(Microsoft.Toolkit.Uwp.UI.Controls.TokenizingTextBox sender, object args)
        {
            if (args is VertragDetailKontakt mieter)
            {
                if (ViewModel.Mieter.Value.Exists(w => w.PersonId == mieter.PersonId))
                {

                }
                else
                {
                    ViewModel.Mieter.Value = ViewModel.Mieter.Value.Add(mieter).ToImmutableList();
                    ViewModel.AlleMieter.Value = ViewModel.AlleMieter.Value.Remove(mieter);
                    App.Walter.MieterSet.Add(new Mieter
                    {
                        PersonId = mieter.PersonId,
                        VertragId = ViewModel.guid,
                    });
                    App.SaveWalter();
                }
            }
        }

        private void MieterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sender.ItemsSource = ViewModel.AlleMieter.Value
                .Where(k => k.Bezeichnung.Trim().ToLower().Contains(sender.Text.Trim().ToLower()))
                .ToImmutableList();
        }
    }
}
