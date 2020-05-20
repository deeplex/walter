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

        private void WohnungSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Suggest(sender, args,
                ViewModel.AlleWohnungen.Value
                    .Where(w => ViewModel.Vermieter.Value?.Id is int vv ? w.BesitzerId.Value == vv : true)
                    .Where(k => k.BezeichnungVoll.Value.Trim().ToLower()
                        .Contains(sender.Text.Trim().ToLower()))
                    .Select(k => k.BezeichnungVoll.Value).ToList(),
                "Wohnung");
        }

        private void VermieterSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
           Suggest(sender, args,
                ViewModel.AlleJuristischePersonen.Value
                    .Where(k => ViewModel.Wohnung?.Value?.BesitzerId.Value is int wb ? k.Id == wb : true)
                    .Where(k => k.Name.Value.Trim().ToLower()
                        .Contains(sender.Text.Trim().ToLower()))
                    .Select(k => k.Name.Value).ToList(),
                "Juristische Person");
        }

        private void MieterSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Suggest(sender, args,
                ViewModel.AlleKontakte.Value
                    .Where(k => k.Name.Value.Trim().ToLower()
                        .Contains(sender.Text.Trim().ToLower()))
                    .Where(k => !ViewModel.Mieter.Value.Exists(m => m.Name.Value == k.Name.Value))
                    .Select(k => k.Name.Value).ToList(),
                "Kontakt");
        }

        private void AnsprechpartnerSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Suggest(sender, args,
                ViewModel.AlleKontakte.Value
                    // If Checkbox => .Where(k => k.JuristischePersonen.Contains(ViewModel.Vermieter.Id))
                    .Where(k => k.Name.Value.Trim().ToLower()
                        .Contains(sender.Text.Trim().ToLower()))
                    .Select(k => k.Name.Value).ToList(),
                "Kontakt");
        }

        private void MieterSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string a)
            {
                if (ViewModel.Mieter.Value.Exists(w => w.Name.Value == a))
                {

                }
                else
                {
                    var m = ViewModel.AlleKontakte.Value.First(k => k.Name.Value == a);
                    ViewModel.Mieter.Value = ViewModel.Mieter.Value.Add(new VertragDetailKontakt(m.Id))
                        // From the longest to the smallest because of XAML I guess;
                        .OrderBy(mw => mw.Name.Value.Length).Reverse().ToImmutableList();
                    sender.Text = "";
                }
            }
        }

        private void WohnungSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string a)
            {
                var wohnung = ViewModel.AlleWohnungen.Value.First(w => w.BezeichnungVoll.Value == a);
                ViewModel.Wohnung.Value = wohnung;
                ViewModel.Versionen.Value.ForEach(v => v.Wohnung.Value = wohnung);
                sender.Text = a;
            }
        }

        private void AnsprechpartnerSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string a)
            {
                var ansprechpartner = ViewModel.AlleKontakte.Value.First(k => k.Name.Value == a);
                ViewModel.Ansprechpartner.Value = ansprechpartner;
                ViewModel.Versionen.Value.ForEach(v => v.Ansprechpartner.Value = ansprechpartner);

                sender.Text = a;
            }
        }

        private void VermieterSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string a)
            {
                ViewModel.Vermieter.Value = ViewModel.AlleJuristischePersonen.Value.First(k => k.Name.Value == a);
                sender.Text = a;
            }
        }

        private void RemoveMieter_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var id = (int)((Button)sender).CommandParameter;
            var mieter = ViewModel.Mieter.Value.Find(m => m.Id == id);
            ViewModel.Mieter.Value = ViewModel.Mieter.Value.Remove(mieter);
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
        }

        private void Betriebskostenabrechnung_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var Jahr = (int)((Button)sender).CommandParameter;
            var b = new Betriebskostenabrechnung(
                ViewModel.Versionen.Value.First().Id, Jahr, new DateTime(Jahr, 1, 1), new DateTime(Jahr, 12, 31));

            var s = Jahr.ToString() + " - " + ViewModel.Wohnung.Value.BezeichnungVoll.Value;

            b.SaveAsDocx(ApplicationData.Current.LocalFolder.Path + @"\" + s + ".docx");
        }
    }
}
