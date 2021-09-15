using Deeplex.Saverwalter.WinUI3.Utils;
using Deeplex.Saverwalter.ViewModels.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{

    public sealed partial class ErhaltungsaufwendungenListViewPage : Page
    {
        public ErhaltungsaufwendungenListViewModel ViewModel = new ErhaltungsaufwendungenListViewModel(App.ViewModel);

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
