using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class UmlageListViewPage : Page
    {
        public UmlageListViewModel ViewModel { get; } = new UmlageListViewModel(App.WalterService, App.NotificationService);

        public UmlageListViewPage()
        {
            InitializeComponent();
            App.Window.Titel.Value = "Betriebskostenrechnungen";

            App.Window.CommandBar.MainContent = new ListCommandBarControl<UmlageListViewModelEntry> { ViewModel = ViewModel };
        }
    }
}
