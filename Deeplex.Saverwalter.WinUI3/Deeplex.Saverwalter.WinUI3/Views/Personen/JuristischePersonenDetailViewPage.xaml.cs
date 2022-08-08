using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class JuristischePersonenDetailViewPage : Page
    {
        public JuristischePersonViewModel ViewModel { get; set; }
        public VertragListViewModel VertragListViewModel { get; set; }
        public KontaktListViewModel MitgliederListViewModel { get; set; }

        public JuristischePersonenDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is JuristischePerson jp)
            {
                ViewModel = App.Container.GetInstance<JuristischePersonViewModel>();
                ViewModel.SetEntity(jp);
                VertragListViewModel = App.Container.GetInstance<VertragListViewModel>();
                VertragListViewModel.SetList(jp);
                MitgliederListViewModel = App.Container.GetInstance<KontaktListViewModel>();
                MitgliederListViewModel.SetList(jp);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new JuristischePersonViewModel(App.NotificationService, App.WalterService);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl<JuristischePerson> { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }
    }
}
