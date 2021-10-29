using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class BetriebskostenRechnungenCommandBarControl : UserControl
    {
        public BetriebskostenRechnungenCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Betriebskostenrechnung"; // TODO Bezeichnung...
        }

        public BetriebskostenrechnungDetailViewModel ViewModel
        {
            get { return (BetriebskostenrechnungDetailViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(BetriebskostenrechnungDetailViewModel),
            typeof(BetriebskostenRechnungenCommandBarControl),
            new PropertyMetadata(null));

        private async void Delete_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (ViewModel.Id != 0)
            {
                await ViewModel.selfDestruct();
            }
            App.Window.AppFrame.GoBack();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Id != 0)
            {
                var next = new BetriebskostenrechnungDetailViewModel(
                    ViewModel.Wohnungen.Value,
                    ViewModel.BetreffendesJahr,
                    ViewModel.Impl,
                    ViewModel.Avm);
                App.Window.Navigate(typeof(BetriebskostenrechnungenDetailPage), next);
            }
        }
    }
}
