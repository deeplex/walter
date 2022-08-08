using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class WohnungListViewPage : Page
    {
        public WohnungListViewModel ViewModel { get; } = App.Container.GetInstance<WohnungListViewModel>();

        public WohnungListViewPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new ListCommandBarControl<WohnungListViewModelEntry> { ViewModel = ViewModel };
            ViewModel.SetList();
        }
    }
}
