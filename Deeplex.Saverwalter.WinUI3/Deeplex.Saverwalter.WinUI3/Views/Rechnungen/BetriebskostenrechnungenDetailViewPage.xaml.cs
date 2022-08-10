using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class BetriebskostenrechnungenDetailViewPage : Page
    {
        public BetriebskostenrechnungDetailViewModel ViewModel { get; } = App.Container.GetInstance<BetriebskostenrechnungDetailViewModel>();

        public BetriebskostenrechnungenDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.SaveWohnungen();
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Betriebskostenrechnung r)
            {
                ViewModel.SetEntity(r);
            }
            base.OnNavigatedTo(e);

            App.Window.CommandBar.MainContent = new DetailCommandBarControl() { ViewModel = ViewModel };
        }
    }
}
