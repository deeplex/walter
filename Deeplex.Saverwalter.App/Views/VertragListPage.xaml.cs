using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class VertragListPage : Page
    {
        public VertragListViewModel ViewModel = new VertragListViewModel();

        public VertragListPage()
        {
            InitializeComponent();
        }

        private void Vertrag_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(VertragDetailViewPage), ViewModel.SelectedVertrag.Value.VertragId,
                new DrillInNavigationTransitionInfo());
        }

        private void AddVertrag_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VertragDetailViewPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
