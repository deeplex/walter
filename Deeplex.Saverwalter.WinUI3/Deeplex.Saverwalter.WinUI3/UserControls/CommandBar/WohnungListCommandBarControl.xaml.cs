using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class WohnungListCommandBarControl : UserControl
    {
        public WohnungListCommandBarControl()
        {
            InitializeComponent();
        }

        public WohnungListViewModel ViewModel
        {
            get { return (WohnungListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(WohnungListViewModel),
            typeof(WohnungListCommandBarControl),
            new PropertyMetadata(null));

        private void AddWohnung_Click(object sender, RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(WohnungDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Filter.Value = ((TextBox)sender).Text;
        }
    }
}
