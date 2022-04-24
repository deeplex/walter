using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ZaehlerListCommandBarControl : UserControl
    {
        public ZaehlerListCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Zähler";
        }

        public ZaehlerListViewModel ViewModel
        {
            get { return (ZaehlerListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(ZaehlerListViewModel),
            typeof(ZaehlerListCommandBarControl),
            new PropertyMetadata(null));

        private void AddZaehler_Click(object sender, RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(ZaehlerDetailViewPage), null,
                new DrillInNavigationTransitionInfo());
        }

        void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Filter.Value = ((TextBox)sender).Text;
        }
    }
}
