using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class KontaktListCommandBarControl : UserControl
    {
        public KontaktListCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Kontakte";
        }

        public KontaktListViewModel ViewModel
        {
            get { return (KontaktListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(KontaktListViewModel),
            typeof(KontaktListCommandBarControl),
            new PropertyMetadata(null));

        private void AddNatuerlichePerson_Click(object sender, RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(NatuerlichePersonDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void AddJuristischePerson_Click(object sender, RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(JuristischePersonenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Filter.Value = ((TextBox)sender).Text;
        }
    }
}
