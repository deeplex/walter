using Deeplex.Saverwalter.App.ViewModels;
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
            else if (e.Parameter is null) // New Contact
            {
                // ViewModel = new KalteBetriebskostenRechnungViewModel();
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
                = ((KeyValuePair<int, ImmutableList<KalteBetriebskostenRechnungJahr>>)
                    ((Pivot)sender).SelectedItem).Key;
        }
    }
}
