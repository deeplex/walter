using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class VertragListViewPage : Page
    {
        public VertragListViewModel ViewModel { get; } = App.Container.GetInstance<VertragListViewModel>();

        public VertragListViewPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
