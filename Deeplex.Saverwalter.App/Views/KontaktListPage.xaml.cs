using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class KontaktListPage : Page
    {
        public KontaktListViewModel ViewModel = new KontaktListViewModel();

        public KontaktListPage()
        {
            InitializeComponent();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var sk = ViewModel.SelectedKontakt;
            if (sk != null)
            {
                var target =
                    sk.Type == typeof(NatuerlichePerson) ? typeof(NatuerlichePersonDetailPage) :
                    sk.Type == typeof(JuristischePerson) ? typeof(JuristischePersonenDetailPage) :
                    null;

                Frame.Navigate(target, ViewModel.SelectedKontakt.Id,
                    new DrillInNavigationTransitionInfo());
            }
        }

        private void AddNatuerlichePerson_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NatuerlichePersonDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void AddJuristischePerson_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(JuristischePersonenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void DataGrid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ViewModel.SelectedKontakt = (e.OriginalSource as FrameworkElement).DataContext as KontaktListEntry;
        }
    }
}
