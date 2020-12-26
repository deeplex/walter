using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
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
                ViewModel = new JuristischePersonViewModel(jp);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new JuristischePersonViewModel();
            }

            base.OnNavigatedTo(e);

            App.ViewModel.Titel.Value = ViewModel.Bezeichnung;

            var EditToggle = new AppBarToggleButton
            {
                Label = "Bearbeiten",
                Icon = new SymbolIcon(Symbol.Edit),
            };
            EditToggle.Click += EditToggle_Click;

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Attach),
                    Command = ViewModel.AttachFile,
                }
            }, new ICommandBarElement[] { EditToggle, Delete });
        }

        private void EditToggle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsInEdit.Value = (sender as AppBarToggleButton).IsChecked ?? false;
        }

        private void SelfDestruct(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.selfDestruct();
            ((Frame)((NavigationView)Frame.Parent).Content).GoBack();
        }
    }
}
