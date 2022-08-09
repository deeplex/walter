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
            ViewModel = App.Container.GetInstance<JuristischePersonViewModel>();
            VertragListViewModel = App.Container.GetInstance<VertragListViewModel>();
            MitgliederListViewModel = App.Container.GetInstance<KontaktListViewModel>();

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
    }
}
