using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class WohnungListViewPage : Page
    {
        public WohnungListViewModel ViewModel { get; } = App.Container.GetInstance<WohnungListViewModel>();

        public WohnungListViewPage()
        {
            InitializeComponent();
            ViewModel.SetList();
            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
