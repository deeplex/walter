using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class NatuerlichePersonDetailViewPage : Page
    {
        public NatuerlichePersonDetailViewModel ViewModel { get; } = App.Container.GetInstance<NatuerlichePersonDetailViewModel>();
        public VertragListViewModel VertragListViewModel { get; } = App.Container.GetInstance<VertragListViewModel>();

        public NatuerlichePersonDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NatuerlichePerson kontakt)
            {
                ViewModel.SetEntity(kontakt);
                VertragListViewModel.SetList(kontakt);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }

        private void Switch_To_Juristische_Person_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Container.GetInstance<INotificationService>().Navigation<JuristischePerson>(null);
        }
    }
}
