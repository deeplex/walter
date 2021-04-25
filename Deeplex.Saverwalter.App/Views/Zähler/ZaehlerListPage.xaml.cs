using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class ZaehlerListPage : Page
    {
        public ZaehlerListPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Zähler";

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {

            });
        }
    }
}
