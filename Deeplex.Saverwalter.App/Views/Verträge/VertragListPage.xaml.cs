using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml;
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

            App.ViewModel.Titel.Value = "Verträge";
            var AddVertrag = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Vertrag hinzufügen"
            };
            AddVertrag.Click += AddVertrag_Click;
            App.ViewModel.RefillCommandContainer(new ICommandBarElement[] { AddVertrag });
        }

        private void AddVertrag_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VertragDetailViewPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
