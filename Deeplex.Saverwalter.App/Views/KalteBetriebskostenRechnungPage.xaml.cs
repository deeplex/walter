using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class KalteBetriebskostenRechnungPage : Page
    {
        public KalteBetriebskostenRechnungViewModel ViewModel { get; set; }

        public KalteBetriebskostenRechnungPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int adresseId)
            {
                ViewModel = new KalteBetriebskostenRechnungViewModel(adresseId);
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }

        private void Vorlage_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(KalteBetriebskostenVorlagePage), ViewModel.AdresseId,
                new DrillInNavigationTransitionInfo());
        }

        private void RemoveRechnung_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var param = (KalteBetriebskostenRechnungJahr)((Button)sender).CommandParameter;
            var j = ViewModel.Jahre.Value[param.Jahr.Value];
            var upd = j.RemoveAll(r => r.Jahr == param.Jahr && r.Typ == param.Typ);
            ViewModel.Jahre.Value = ViewModel.Jahre.Value.SetItem(param.Jahr.Value, upd);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedJahr
                = ViewModel.IsNotEmpty ? ((KeyValuePair<int, ImmutableList<KalteBetriebskostenRechnungJahr>>)
                    ((Pivot)sender).SelectedItem).Key : 0;
        }

        private void AddRechnung_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var jahr = ViewModel.SelectedJahr;
                sender.ItemsSource = Enum.GetValues(typeof(KalteBetriebskosten)).Cast<KalteBetriebskosten>()
                    .ToList()
                    .Where(r => jahr > 0 ? !ViewModel.Jahre.Value[jahr].Exists(j => j.Typ.Value == r) : false)
                    .Select(k => new AddRechnung
                    {
                        Typ = k,
                        Bezeichnung = k.ToDescriptionString(),
                    })
                    .Where(k => k.Bezeichnung.Contains(sender.Text)).ToList();
            }
        }

        private void AddRechnung_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is AddRechnung a)
            {
                var jahr = ViewModel.SelectedJahr;
                var j = ViewModel.Jahre.Value[jahr];
                var upd = j.Add(new KalteBetriebskostenRechnungJahr(a.Typ, jahr));
                ViewModel.Jahre.Value = ViewModel.Jahre.Value.SetItem(jahr, upd);
                sender.IsSuggestionListOpen = false;
                sender.Text = "";
            }
        }

        private class AddRechnung
        {
            public KalteBetriebskosten Typ { get; set; }
            public string Bezeichnung { get; set; }
        }
    }
}
