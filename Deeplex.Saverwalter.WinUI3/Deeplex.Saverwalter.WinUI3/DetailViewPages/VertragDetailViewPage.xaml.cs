using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class VertragDetailViewPage : Page
    {
        public VertragDetailViewModel ViewModel { get; } = App.Container.GetInstance<VertragDetailViewModel>();
        public BetriebskostenrechnungListViewModel BetriebskostenListViewModel { get; } = App.Container.GetInstance<BetriebskostenrechnungListViewModel>();
        public UmlageListViewModel UmlageListViewModel { get; } = App.Container.GetInstance<UmlageListViewModel>();
        public VertragVersionListViewModel VersionListViewModel { get; } = App.Container.GetInstance<VertragVersionListViewModel>();

        public VertragDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Vertrag v)
            {
                ViewModel.SetEntity(v);
                BetriebskostenListViewModel.SetList(v);
                UmlageListViewModel.SetList(v);
                VersionListViewModel.SetList(v);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl() { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }

        private void Betriebskostenabrechnung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(BetriebskostenrechnungPrintViewPage), ViewModel.Entity,
                new DrillInNavigationTransitionInfo());
        }
    }
}
