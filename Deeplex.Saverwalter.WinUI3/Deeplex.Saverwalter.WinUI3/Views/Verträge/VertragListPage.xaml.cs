using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Utils;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

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
