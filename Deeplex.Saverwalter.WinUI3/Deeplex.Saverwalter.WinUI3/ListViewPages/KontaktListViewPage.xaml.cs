using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class KontaktListViewPage : Page
    {
        public KontaktListViewModel ViewModel { get; } = App.Container.GetInstance<KontaktListViewModel>();

        public KontaktListViewPage()
        {
            InitializeComponent();
            ViewModel.SetList();
            App.Window.CommandBar.MainContent = new ListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
