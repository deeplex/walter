using Deeplex.Saverwalter.App.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class WohnungListPage : Page
    {
        public List<WohnungListViewModel> ViewModel { get; set; }

        public List<IGrouping<string, WohnungListViewModel>> AdresseGroup
            => ViewModel.GroupBy(w => w.Anschrift.Value).ToList();

        public WohnungListPage()
        {
            InitializeComponent();
            ViewModel = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListViewModel(w))
                .ToList();
        }

        private void Wohnung_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(WohnungDetailPage), ((Button)sender).CommandParameter,
                new DrillInNavigationTransitionInfo());
        }

        private void AddWohnung_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WohnungDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void Rechnungen_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(KalteBetriebskostenRechnungPage),
                AdresseViewModel.GetAdresseIdByAnschrift((string)((Button)sender).CommandParameter),
                new DrillInNavigationTransitionInfo());
        }
    }
}
