using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class BetriebskostenRechnungenListCommandBarControl : UserControl
    {
        public BetriebskostenRechnungenListCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Betriebskostenrechnungen";
        }

        public BetriebskostenRechnungenListViewModel ViewModel
        {
            get { return (BetriebskostenRechnungenListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(BetriebskostenRechnungenListViewModel),
            typeof(BetriebskostenRechnungenListCommandBarControl),
            new PropertyMetadata(null));

        private void AddBetriebskostenrechnung_Click(object sender, RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(BetriebskostenrechnungenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Filter.Value = ((TextBox)sender).Text;
        }
    }
}
