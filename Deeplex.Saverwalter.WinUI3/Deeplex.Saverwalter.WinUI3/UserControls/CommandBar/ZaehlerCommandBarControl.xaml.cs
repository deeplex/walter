using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ZaehlerCommandBarControl : UserControl
    {
        public ZaehlerCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Zähler"; // TODO Bezeichnung...
        }

        public ZaehlerDetailViewModel ViewModel
        {
            get { return (ZaehlerDetailViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(ZaehlerDetailViewModel),
            typeof(ZaehlerCommandBarControl),
            new PropertyMetadata(null));

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SelfDestruct();
            App.Window.AppFrame.GoBack();
        }
    }
}
