using Deeplex.Saverwalter.WinUI3.Utils;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class WohnungListPage : Page
    {
        public WohnungListViewModel ViewModel = new WohnungListViewModel(App.ViewModel);

        public WohnungListPage()
        {
            InitializeComponent();

            App.ViewModel.Titel.Value = "Mietobjekte";
            var AddWohnung = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Wohnung hinzufügen",
            };
            AddWohnung.Click += AddWohnung_Click;
            App.ViewModel.RefillCommandContainer(new ICommandBarElement[] { Elements.Filter(ViewModel), AddWohnung });
        }

        private void AddWohnung_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WohnungDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
