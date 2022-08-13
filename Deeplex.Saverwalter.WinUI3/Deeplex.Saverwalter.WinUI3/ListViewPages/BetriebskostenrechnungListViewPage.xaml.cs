using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class BetriebskostenrechnungListViewPage : Page
    {
        public BetriebskostenrechnungListViewModel ViewModel { get; } = App.Container.GetInstance<BetriebskostenrechnungListViewModel>();

        public BetriebskostenrechnungListViewPage()
        {
            InitializeComponent();
            ViewModel.SetList();
            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
