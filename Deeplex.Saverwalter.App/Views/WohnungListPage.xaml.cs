using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class WohnungListPage : Page
    {
        public WohnungListViewModel ViewModel = new WohnungListViewModel();

        public WohnungListPage()
        {
            InitializeComponent();

            App.ViewModel.Titel.Value = "Mietobjekte";
            var AddWohnung = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Wohnung hinzufügen",
            };
            AddWohnung.Click += AddWohnung_Click;
            App.ViewModel.RefillCommandContainer(new[] { AddWohnung });
        }

        private void Wohnung_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var wohnung = App.Walter.Wohnungen.Find((sender as Button).CommandParameter);
            App.ViewModel.Navigate(typeof(WohnungDetailPage), wohnung);
        }

        private void AddWohnung_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WohnungDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void AddRest_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = (Button)sender;

            double getDoubleValue(int idx) => Convert.ToDouble(((NumberBox)((StackPanel)button.Parent).Children[idx]).Text);
            int getIntValue(int idx) => Convert.ToInt32(((NumberBox)((StackPanel)button.Parent).Children[idx]).Text);

            var Wohnflaeche = getDoubleValue(0);
            var Nutzflaeche = getDoubleValue(1);
            var Nutzeinheit = getIntValue(2);
            var Personenzahl = getIntValue(3);

            var adr = (WohnungListAdresse)button.CommandParameter;
            var w = new Wohnung
            {
                Wohnflaeche = Wohnflaeche - adr.GesamtWohnflaeche,
                Nutzflaeche = Nutzflaeche - adr.GesamtNutzflaeche,
                Nutzeinheit = Nutzeinheit - adr.GesamtNutzeinheiten,
                AdresseId = adr.Id,
                Bezeichnung = "Übrige Einheiten",
            };
            var v = new Vertrag
            {
                Beginn = new DateTime(1970, 1, 1).AsUtcKind(),
                Personenzahl = Personenzahl,
                Wohnung = w,
            };
            App.Walter.Wohnungen.Add(w);
            App.Walter.Vertraege.Add(v);
            App.SaveWalter();
        }
    }
}
