using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.App.ViewModels.Rechnungen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views.Rechnungen
{

    public sealed partial class ErhaltungsaufwendungenListViewPage : Page
    {
        public ErhaltungsaufwendungenListViewModel ViewModel = new ErhaltungsaufwendungenListViewModel();

        public ErhaltungsaufwendungenListViewPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Erhaltungsaufwendungen";

            var AddErhaltungsaufwendung = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Erhaltungsaufwendung hinzufügen",
            };
            AddErhaltungsaufwendung.Click += AddErhaltungsaufwendung_Click;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                Elements.Filter(ViewModel),
                AddErhaltungsaufwendung,
            });
        }

        private void AddErhaltungsaufwendung_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ErhaltungsaufwendungenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
