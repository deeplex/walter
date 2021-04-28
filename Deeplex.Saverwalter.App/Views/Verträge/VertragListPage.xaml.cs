using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Utils.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class VertragListPage : Page
    {
        public VertragListViewModel ViewModel = new VertragListViewModel();
        public ObservableProperty<bool> OnlyActive = new ObservableProperty<bool>();

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

            var ShowActive = new AppBarToggleButton
            {
                Label = "Nur aktive Verträge zeigen.",
            };
            ShowActive.Click += ShowActive_Click; ;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[] { ShowActive, Elements.Filter(ViewModel), AddVertrag });
        }

        private void ShowActive_Click(object sender, RoutedEventArgs e)
        {
            OnlyActive.Value = ((AppBarToggleButton)sender).IsChecked ?? false;
        }

        private void AddVertrag_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VertragDetailViewPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
