using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{
    public sealed partial class ErhaltungsaufwendungenDetailViewPage : Page
    {
        public ErhaltungsaufwendungenDetailViewModel ViewModel { get; set; }

        public ErhaltungsaufwendungenDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int id)
            {
                ViewModel = new ErhaltungsaufwendungenDetailViewModel(App.WalterService.ctx.Erhaltungsaufwendungen.Find(id), App.NotificationService, App.WalterService);
            }
            else if (e.Parameter is ErhaltungsaufwendungenDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else if (e.Parameter is Erhaltungsaufwendung r)
            {
                ViewModel = new ErhaltungsaufwendungenDetailViewModel(r, App.NotificationService, App.WalterService);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new ErhaltungsaufwendungenDetailViewModel(App.NotificationService, App.WalterService);
            }

            App.Window.CommandBar.MainContent = new ErhaltungsaufwendungenCommandBarControl { ViewModel = ViewModel };

            base.OnNavigatedTo(e);
        }

        private void AddQuickPerson_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.QuickPerson.Value != "" && ViewModel.QuickPerson.Value != null)
            {
                var j = new Model.JuristischePerson()
                {
                    Bezeichnung = ViewModel.QuickPerson.Value,
                    isHandwerker = true,
                };
                App.WalterService.ctx.JuristischePersonen.Add(j);
                App.WalterService.SaveWalter();

                var kl = new KontaktListViewModelEntry(j);
                ViewModel.Personen.Value = ViewModel.Personen.Value.Add(kl);
                ViewModel.Aussteller = kl;
                ViewModel.QuickPerson.Value = "";

                if (QuickPersonFlyout.Flyout is Flyout f)
                {
                    f.Hide();
                }
            }
        }
    }
}
