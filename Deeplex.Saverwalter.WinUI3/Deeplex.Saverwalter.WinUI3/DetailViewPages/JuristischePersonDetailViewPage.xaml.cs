using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class JuristischePersonDetailViewPage : Page
    {
        public JuristischePersonDetailViewModel ViewModel { get; } = App.Container.GetInstance<JuristischePersonDetailViewModel>();
        public VertragListViewModel VertragListViewModel { get; } = App.Container.GetInstance<VertragListViewModel>();
        public KontaktListViewModel MitgliederListViewModel { get; } = App.Container.GetInstance<KontaktListViewModel>();

        public JuristischePersonDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is JuristischePerson jp)
            {
                ViewModel.SetEntity(jp);
                VertragListViewModel.SetList(jp);
                MitgliederListViewModel.SetList(jp);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }

        private void Switch_To_Natuerliche_Person_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Container.GetInstance<INotificationService>().Navigation<NatuerlichePerson>(null);
        }
    }
}
