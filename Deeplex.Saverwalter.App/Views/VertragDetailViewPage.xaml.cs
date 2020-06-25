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
            if (ViewModel.Wohnung != null)
            {
                WohnungComboBox.SelectedIndex = ViewModel.AlleWohnungen.FindIndex(w => w.Id == ViewModel.Wohnung.Id);
            }
            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            if (ViewModel.Ansprechpartner != null) // TODO this is an ugly way to accomplish initial loading of Besitzer in GUI
            {
                AnsprechpartnerSuggest.Text = ViewModel.Ansprechpartner.Bezeichnung;
            }
            base.OnNavigatedTo(e);
        }

        private void MieterSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sender.ItemsSource = ViewModel.AlleMieter.Where(k => k.isMieter == true)
                    // If Checkbox => .Where(k => k.JuristischePersonen.Contains(ViewModel.Vermieter.Id))
                    .Where(k => k.Bezeichnung.Trim().ToLower()
                        .Contains(sender.Text.Trim().ToLower()))
                    .ToImmutableList();
        }

        private void AnsprechpartnerSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sender.ItemsSource = ViewModel.AlleKontakte
                    // If Checkbox => .Where(k => k.JuristischePersonen.Contains(ViewModel.Vermieter.Id))
                    .Where(k => k.Bezeichnung.Trim().ToLower().Contains(sender.Text.Trim().ToLower()))
                    .ToImmutableList();
        }

        private void MieterSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is VertragDetailKontakt a)
            {
                if (ViewModel.Mieter.Value.Exists(w => w.PersonId == a.PersonId))
                {

                }
                else
                {
                    var m = ViewModel.AlleMieter.First(k => k.PersonId == a.PersonId);
                    App.Walter.MieterSet.Add(new Mieter
                    {
                        PersonId = m.PersonId,
                        VertragId = ViewModel.guid,
                    });
                    App.Walter.SaveChanges();
                    ViewModel.Mieter.Value = ViewModel.Mieter.Value.Add(new VertragDetailKontakt(m.PersonId))
                        // From the longest to the smallest because of XAML I guess;
                        .OrderBy(mw => mw.Bezeichnung.Length).Reverse().ToImmutableList();
                    sender.Text = "";
                }
            }
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
                App.Walter.SaveChanges();
            }
        }

        private void RemoveMieter_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var id = (Guid)((Button)sender).CommandParameter;
            var mieter = ViewModel.Mieter.Value.Find(m => m.PersonId == id);
            ViewModel.Mieter.Value = ViewModel.Mieter.Value.Remove(mieter);
            App.Walter.MieterSet.Remove(
                App.Walter.MieterSet.First(
                    m => m.VertragId == ViewModel.guid && m.PersonId == id));
            App.Walter.SaveChanges();
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
            ViewModel.IsInEdit.Value = EditToggle.IsChecked ?? false;
        }
    }
}
