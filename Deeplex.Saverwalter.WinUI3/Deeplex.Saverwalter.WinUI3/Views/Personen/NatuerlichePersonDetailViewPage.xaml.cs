using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class NatuerlichePersonDetailViewPage : Page
    {
        public NatuerlichePersonViewModel ViewModel { get; set; }
        public VertragListViewModel VertragListViewModel { get; set; }

        public NatuerlichePersonDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NatuerlichePerson kontakt)
            {
                ViewModel = App.Container.GetInstance<NatuerlichePersonViewModel>();
                ViewModel.SetEntity(kontakt);
                VertragListViewModel = App.Container.GetInstance<VertragListViewModel>();
                VertragListViewModel.SetList(kontakt);
            }
            else if (e.Parameter is null) // New Contact
            {
                ViewModel = new NatuerlichePersonViewModel(App.NotificationService, App.WalterService);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl<NatuerlichePerson> { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }
    }
}
