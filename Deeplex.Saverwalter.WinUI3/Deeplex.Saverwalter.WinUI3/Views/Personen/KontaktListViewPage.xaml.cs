using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class KontaktListViewPage : Page
    {
        public KontaktListViewModel ViewModel { get; } = new KontaktListViewModel(App.WalterService);

        public KontaktListViewPage()
        {
            InitializeComponent();
            App.Window.Titel.Value = "Kontakte";

            App.Window.CommandBar.MainContent = new KontaktListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
