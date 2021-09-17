﻿using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class BetriebskostenRechnungenListViewPage : Page
    {
        public BetriebskostenRechnungenListViewModel ViewModel = new BetriebskostenRechnungenListViewModel(App.ViewModel);

        public BetriebskostenRechnungenListViewPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Betriebskostenrechnungen";

            App.Window.CommandBar.MainContent = new BetriebskostenRechnungenListCommandBarControl { ViewModel = ViewModel };
        }
    }
}
