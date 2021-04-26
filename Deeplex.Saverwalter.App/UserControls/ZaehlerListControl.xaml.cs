using Deeplex.Saverwalter.App.ViewModels.Zähler;
using Deeplex.Saverwalter.App.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class ZaehlerListControl : UserControl
    {
        public ZaehlerListViewModel ViewModel { get; set; }

        public ZaehlerListControl()
        {
            InitializeComponent();
            ViewModel = new ZaehlerListViewModel();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedZaehler != null)
            {
                App.ViewModel.Navigate(
                    typeof(ZaehlerDetailPage),
                    App.Walter.ZaehlerSet.Find(ViewModel.SelectedZaehler.Id));
            }
        }
    }
}
