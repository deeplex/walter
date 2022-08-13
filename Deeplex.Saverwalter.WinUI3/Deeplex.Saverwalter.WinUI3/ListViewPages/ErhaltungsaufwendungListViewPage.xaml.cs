using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{

    public sealed partial class ErhaltungsaufwendungListViewPage : Page
    {
        public ErhaltungsaufwendungListViewModel ViewModel { get; } = App.Container.GetInstance<ErhaltungsaufwendungListViewModel>();

        public ErhaltungsaufwendungListViewPage()
        {
            InitializeComponent();
            ViewModel.SetList();
            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
