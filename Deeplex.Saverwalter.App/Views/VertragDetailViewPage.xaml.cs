using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Print;
using System;
using System.Collections.Generic;
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
            if (ViewModel.Wohnung.Value != null)
            {
                WohnungComboBox.SelectedIndex = ViewModel.AlleWohnungen.FindIndex(w => w.Id == ViewModel.Wohnung.Value.Id);
            }
            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }

        private void Suggest(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args,
            List<string> suggestions, string add)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = suggestions.Count > 0 ? suggestions
                    : new List<string> { sender.Text + " (" + add + " hinzufügen)" };
            }
        }
     
        private void MieterSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Suggest(sender, args,
                ViewModel.AlleKontakte
                    .Where(k => k.Name.Trim().ToLower()
                        .Contains(sender.Text.Trim().ToLower()))
                    .Where(k => !ViewModel.Mieter.Value.Exists(m => m.Name == k.Name))
                    .Select(k => k.Name).ToList(),
                "Kontakt");
        }

        private void AnsprechpartnerSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Suggest(sender, args,
                ViewModel.AlleKontakte
                    // If Checkbox => .Where(k => k.JuristischePersonen.Contains(ViewModel.Vermieter.Id))
                    .Where(k => k.Name.Trim().ToLower()
                        .Contains(sender.Text.Trim().ToLower()))
                    .Select(k => k.Name).ToList(),
                "Kontakt");
        }

        private void MieterSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string a)
            {
                if (ViewModel.Mieter.Value.Exists(w => w.Name == a))
                {

                }
                else
                {
                    var m = ViewModel.AlleKontakte.First(k => k.Name == a);
                    App.Walter.MieterSet.Add(new Mieter
                    {
                        KontaktId = m.Id,
                        VertragId = ViewModel.guid,
                    });
                    App.Walter.SaveChanges();
                    ViewModel.Mieter.Value = ViewModel.Mieter.Value.Add(new VertragDetailKontakt(m.Id))
                        // From the longest to the smallest because of XAML I guess;
                        .OrderBy(mw => mw.Name.Length).Reverse().ToImmutableList();
                    sender.Text = "";
                }
            }
        }

        private void AnsprechpartnerSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string a)
            {
                var ansprechpartner = ViewModel.AlleKontakte.First(k => k.Name == a);
                var entity = App.Walter.Kontakte.Find(ansprechpartner.Id);

                ViewModel.Ansprechpartner.Value = ansprechpartner;
                ViewModel.Versionen.Value.ForEach(v => v.Ansprechpartner.Value = ansprechpartner);
                App.Walter.Vertraege.Where(vs => vs.VertragId == ViewModel.guid).ToList().ForEach(vs =>
                {
                    vs.Ansprechpartner = entity;
                });
                sender.Text = a;
                App.Walter.SaveChanges();
            }
        }

        private void RemoveMieter_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var id = (int)((Button)sender).CommandParameter;
            var mieter = ViewModel.Mieter.Value.Find(m => m.Id == id);
            ViewModel.Mieter.Value = ViewModel.Mieter.Value.Remove(mieter);
            App.Walter.MieterSet.Remove(
                App.Walter.MieterSet.First(
                    m => m.VertragId == ViewModel.guid && m.KontaktId == id));
            App.Walter.SaveChanges();
        }

        private void RemoveDate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Ende.Value = null;
            ViewModel.Versionen.Value.Last().Ende.Value = null;
        }

        private void RemoveMiete_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var miete = (VertragDetailMiete)((Button)sender).CommandParameter;
            ViewModel.Mieten.Value = ViewModel.Mieten.Value.Remove(miete).ToImmutableList();
            App.Walter.Mieten.Remove(miete.Entity);
            App.Walter.SaveChanges();
        }

        private void Betriebskostenabrechnung_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var Jahr = (int)((Button)sender).CommandParameter;
            var b = new Betriebskostenabrechnung(
                ViewModel.Versionen.Value.First().Id, Jahr, new DateTime(Jahr, 1, 1), new DateTime(Jahr, 12, 31));

            var s = Jahr.ToString() + " - " + ViewModel.Wohnung.Value.BezeichnungVoll;

            b.SaveAsDocx(ApplicationData.Current.LocalFolder.Path + @"\" + s + ".docx");
        }

        private void EditToggle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsInEdit.Value = EditToggle.IsChecked ?? false;
        }

        private void WohnungComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var w = (VertragDetailWohnung)WohnungComboBox.SelectedItem;
            ViewModel.Wohnung.Value = w;
            ViewModel.Versionen.Value.ForEach(vs =>
            {
                vs.Wohnung.Value = w;
                var v = App.Walter.Vertraege.Find(vs.Id);
                v.Wohnung = w.Entity;
                App.Walter.Vertraege.Update(v);
            });
            App.Walter.SaveChanges();
        }
    }
}
