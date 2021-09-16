using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

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

            App.ViewModel.Titel.Value = ViewModel.Bezeichnung;
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.GetEntity, App.Impl, App.ViewModel));

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.Window.RefillCommandContainer(
                new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });

            base.OnNavigatedTo(e);
        }

        private async void SelfDestruct(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.selfDestruct();
            Frame.GoBack();
        }
    }
}
