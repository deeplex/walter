using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class KontaktListPage : Page
    {

        public MainViewModel ViewModel => App.ViewModel;

        public KontaktListPage()
        {
            InitializeComponent();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedKontakt.Value != null)
            {
                Frame.Navigate(typeof(KontaktDetailPage), App.ViewModel.SelectedKontakt.Value.Id,
                    new DrillInNavigationTransitionInfo());
            }
        }
    }
}
