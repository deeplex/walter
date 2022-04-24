using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class VertragListViewPage : Page
    {
        public VertragListViewModel ViewModel { get; } = new VertragListViewModel(App.WalterService);

        public VertragListViewPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new VertragListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
