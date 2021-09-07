﻿using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class BetriebskostenRechnungenListViewPage : Page
    {
        public BetriebskostenRechnungenListViewModel ViewModel = new BetriebskostenRechnungenListViewModel(App.ViewModel);

        public BetriebskostenRechnungenListViewPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Betriebskostenrechnungen";

            var AddBetriebskostenRechnung = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Betriebskostenrechnung hinzufügen",
            };
            AddBetriebskostenRechnung.Click += AddBetriebskostenWohnung_Click;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                Elements.Filter(ViewModel),
                AddBetriebskostenRechnung,
            });
        }

        private void AddBetriebskostenWohnung_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BetriebskostenrechnungenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
