using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class JuristischePersonCommandBarControl : UserControl
    {
        public JuristischePersonCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Persondetails"; // TODO Bezeichnung...
        }

        public JuristischePersonViewModel ViewModel
        {
            get { return (JuristischePersonViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(JuristischePersonViewModel),
            typeof(JuristischePersonCommandBarControl),
            new PropertyMetadata(null));

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.selfDestruct();
            App.Window.AppFrame.GoBack();
        }
    }
}
