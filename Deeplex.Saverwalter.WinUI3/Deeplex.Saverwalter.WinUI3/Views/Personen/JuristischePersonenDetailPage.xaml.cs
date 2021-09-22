using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class JuristischePersonenDetailPage : Page
    {
        public JuristischePersonViewModel ViewModel { get; set; }

        public JuristischePersonenDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is JuristischePerson jp)
            {
                ViewModel = new JuristischePersonViewModel(jp, App.Impl, App.ViewModel);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new JuristischePersonViewModel(App.Impl, App.ViewModel);
            }

            App.Window.CommandBar.MainContent = new JuristischePersonCommandBarControl { ViewModel = ViewModel };
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
