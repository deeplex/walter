using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class WohnungListPage : Page
    {
        public WohnungListViewModel ViewModel = new WohnungListViewModel();

        public WohnungListPage()
        {
            InitializeComponent();

            App.ViewModel.Titel.Value = "Mietobjekte";
            var AddWohnung = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Wohnung hinzufügen",
            };
            AddWohnung.Click += AddWohnung_Click;
            App.ViewModel.RefillCommandContainer(new[] { AddWohnung });
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Navigate(typeof(WohnungDetailPage), ViewModel.SelectedWohnung.Value.Entity);
        }

        private void AddWohnung_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WohnungDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
