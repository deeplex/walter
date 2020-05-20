using Deeplex.Saverwalter.App.ViewModels;
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
            Frame.Navigate(typeof(KalteBetriebskostenRechnungPage), (int)((Button)sender).CommandParameter,
                new DrillInNavigationTransitionInfo());
        }

        private void Adresse_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.IsInEdit.Value = true;
        }
    }
}
