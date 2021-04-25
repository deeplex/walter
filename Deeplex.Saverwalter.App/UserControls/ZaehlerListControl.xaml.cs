using Deeplex.Saverwalter.App.ViewModels.Zähler;
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
    }
}
