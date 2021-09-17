using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class NatuerlichePersonCommandBarControl : UserControl
    {
        public NatuerlichePersonCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Persondetails"; // TODO Bezeichnung...
        }

        public NatuerlichePersonViewModel ViewModel
        {
            get { return (NatuerlichePersonViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(NatuerlichePersonViewModel),
            typeof(NatuerlichePersonCommandBarControl),
            new PropertyMetadata(null));

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.selfDestruct();
            App.Window.AppFrame.GoBack();
        }
    }
}
