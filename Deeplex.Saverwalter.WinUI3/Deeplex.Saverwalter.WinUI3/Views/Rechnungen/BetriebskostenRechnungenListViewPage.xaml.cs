using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class BetriebskostenRechnungenListViewPage : Page
    {
        public BetriebskostenRechnungenListViewModel ViewModel { get; } = new BetriebskostenRechnungenListViewModel(App.WalterService);

        public BetriebskostenRechnungenListViewPage()
        {
            InitializeComponent();
            App.Window.Titel.Value = "Betriebskostenrechnungen";

            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
