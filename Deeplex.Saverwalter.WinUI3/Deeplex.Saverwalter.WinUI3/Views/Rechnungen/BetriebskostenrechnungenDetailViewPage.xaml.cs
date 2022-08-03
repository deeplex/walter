﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class BetriebskostenrechnungenDetailViewPage : Page
    {
        public BetriebskostenrechnungDetailViewModel ViewModel { get; private set; }

        public BetriebskostenrechnungenDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.SaveWohnungen();
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Betriebskostenrechnung r)
            {
                ViewModel = new(r, App.NotificationService, App.WalterService);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new BetriebskostenrechnungDetailViewModel(App.NotificationService, App.WalterService);
            }

            base.OnNavigatedTo(e);

            App.Window.CommandBar.MainContent = new DetailCommandBarControl() { ViewModel = ViewModel };
        }
    }
}
