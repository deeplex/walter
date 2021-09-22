using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class NatuerlichePersonDetailPage : Page
    {
        public NatuerlichePersonViewModel ViewModel { get; set; }

        public NatuerlichePersonDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NatuerlichePerson kontakt)
            {
                ViewModel = new NatuerlichePersonViewModel(kontakt, App.Impl, App.ViewModel);
            }
            else if (e.Parameter is null) // New Contact
            {
                ViewModel = new NatuerlichePersonViewModel(App.Impl, App.ViewModel);
            }

            App.Window.CommandBar.MainContent = new NatuerlichePersonCommandBarControl { ViewModel = ViewModel };
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }

        private void Erhaltungsaufwendung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(
                typeof(ErhaltungsaufwendungenPrintPage),
                ViewModel.Entity,
                new DrillInNavigationTransitionInfo());
        }
    }
}
