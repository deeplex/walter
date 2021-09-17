using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class KontaktListPage : Page
    {
        public KontaktListViewModel ViewModel = new KontaktListViewModel(App.ViewModel);

        public KontaktListPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Kontakte";

            App.Window.CommandBar.MainContent = new KontaktListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
