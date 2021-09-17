using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class VertragListPage : Page
    {
        public VertragListViewModel ViewModel = new VertragListViewModel(App.ViewModel);

        public VertragListPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new VertragListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
