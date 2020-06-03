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
            var sk = ViewModel.SelectedKontakt.Value;
            if (sk != null)
            {
                var target =
                    sk.Type == typeof(Kontakt) ? typeof(KontaktDetailPage) :
                    sk.Type == typeof(JuristischePerson) ? typeof(JuristischePersonenDetailPage) :
                    null;

                Frame.Navigate(target, ViewModel.SelectedKontakt.Value.Id,
                    new DrillInNavigationTransitionInfo());
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(KontaktDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
} 
