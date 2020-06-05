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
                    sk.Type == typeof(Kontakt) ? typeof(KontaktDetailPage) :
                    sk.Type == typeof(JuristischePerson) ? typeof(JuristischePersonenDetailPage) :
                    null;

                Frame.Navigate(target, ViewModel.SelectedKontakt.Id,
                    new DrillInNavigationTransitionInfo());
            }
        }

        private void AddKontakt_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(KontaktDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void AddJuristischePeron_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(JuristischePersonenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
} 
