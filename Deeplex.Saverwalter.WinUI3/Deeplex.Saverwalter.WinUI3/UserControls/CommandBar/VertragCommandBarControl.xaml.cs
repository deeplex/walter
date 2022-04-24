using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class VertragCommandBarControl : UserControl
    {
        public VertragCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Vertragdetails"; // TODO Bezeichnung...
        }

        public VertragDetailViewModel ViewModel
        {
            get { return (VertragDetailViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(VertragDetailViewModel),
            typeof(VertragCommandBarControl),
            new PropertyMetadata(null));


        private async void Delete_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.SelfDestruct();
            App.Window.AppFrame.GoBack();
        }

        private void Betriebskostenabrechnung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(BetriebskostenrechnungPrintViewPage), ViewModel.Entity,
                new DrillInNavigationTransitionInfo());
        }
    }
}
