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
            else if (e.Parameter is VertragDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else // If invoked using "Add"
            {
                ViewModel = new VertragDetailViewModel();
            }

            if (ViewModel.Ansprechpartner != null) // TODO this is an ugly way to accomplish initial loading of Besitzer in GUI
            {
                AnsprechpartnerSuggest.Text = ViewModel.Ansprechpartner.ToString();
            }
            App.ViewModel.Titel.Value = "Vertragdetails";

            var Primary = new ICommandBarElement[] {
                BetriebskostenAbrechnungsButton(),
                new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Attach),
                    Command = ViewModel.AttachFile,
                }
            };

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += Delete_Click;

            var Secondary = new ICommandBarElement[]
            {
                Delete,
            };

            App.ViewModel.RefillCommandContainer(Primary, Secondary);

            base.OnNavigatedTo(e);
        }

        private void Delete_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SelfDestruct();
            Frame.GoBack();
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

        private void AnsprechpartnerSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sender.ItemsSource = ViewModel.AlleKontakte
                    // If Checkbox => .Where(k => k.JuristischePersonen.Contains(ViewModel.Vermieter.Id))
                    .Where(k => k.ToString().Trim().ToLower().Contains(sender.Text.Trim().ToLower()))
                    .ToImmutableList();
        }

        private void AnsprechpartnerSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is KontaktListEntry a)
            {
                var ansprechpartner = ViewModel.AlleKontakte.First(k => k.Guid == a.Guid);

                ViewModel.Ansprechpartner = ansprechpartner;
                ViewModel.Versionen.Value.ForEach(v => v.Ansprechpartner = ansprechpartner);
                App.Walter.Vertraege.Where(vs => vs.VertragId == ViewModel.guid).ToList().ForEach(vs =>
                {
                    vs.AnsprechpartnerId = a.Guid;
                });
                App.SaveWalter();
            }
        }

        private void RemoveDate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Ende = null;
            ViewModel.Versionen.Value.Last().Ende = null;
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

            var AuflistungMieter = string.Join(", ", App.Walter.MieterSet
                .Where(m => m.VertragId == ViewModel.guid).ToList()
                .Select(a => App.Walter.FindPerson(a.PersonId).Bezeichnung));

            var s = Jahr.ToString() + " - " + ViewModel.Wohnung.ToString() + " - " + AuflistungMieter;

            var worked = b.SaveAsDocx(ApplicationData.Current.LocalFolder.Path + @"\" + s + ".docx");
            var text = worked ? "Datei gespeichert als: " + s : "Datei konnte nicht gespeichert werden.";
            App.ViewModel.ShowAlert(text, 5000);
        }

        private void AddMieter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((KontaktListEntry)((ComboBox)sender).SelectedItem)?.Guid is Guid guid)
            {
                ((ComboBox)sender).SelectedItem = null;
                ViewModel.Mieter.Value = ViewModel.Mieter.Value.Add(new KontaktListEntry(guid));
                AddMieter_Flyout.Hide();
                ViewModel.UpdateMieterList();
                App.Walter.MieterSet.Add(new Mieter()
                {
                    VertragId = ViewModel.guid,
                    PersonId = guid,
                });
                App.SaveWalter();
            }
        }
    }
}
