using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using System;
using Microsoft.UI.Xaml.Controls;
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
                ViewModel = new JuristischePersonViewModel(jp, App.ViewModel);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new JuristischePersonViewModel(App.ViewModel);
            }

            App.ViewModel.Titel.Value = ViewModel.Bezeichnung;
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.ViewModel));

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.ViewModel.RefillCommandContainer(
                new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });

            base.OnNavigatedTo(e);
        }

        private async void SelfDestruct(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (await App.ViewModel.Confirmation())
                {
                    ViewModel.selfDestruct();
                    Frame.GoBack();
                }
            }
            catch
            {

            }
        }

        private void AddMitglied_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (ViewModel.AddMitglied.Value?.Guid is Guid guid)
            {
                AddMitglied_Flyout.Hide();
                App.Walter.JuristischePersonenMitglieder.Add(new JuristischePersonenMitglied()
                {
                    JuristischePersonId = ViewModel.Id,
                    PersonId = ViewModel.AddMitglied.Value.Guid,
                });
                App.SaveWalter();
                ViewModel.UpdateListen();
            }
        }
    }
}
