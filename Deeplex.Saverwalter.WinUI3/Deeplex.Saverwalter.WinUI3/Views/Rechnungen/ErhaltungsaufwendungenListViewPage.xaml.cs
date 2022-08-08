﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{

    public sealed partial class ErhaltungsaufwendungenListViewPage : Page
    {
        public ErhaltungsaufwendungenListViewModel ViewModel { get; } = new ErhaltungsaufwendungenListViewModel(App.WalterService, App.NotificationService);

        public ErhaltungsaufwendungenListViewPage()
        {
            InitializeComponent();

            App.Window.CommandBar.MainContent = new ListCommandBarControl<ErhaltungsaufwendungenListViewModelEntry> { ViewModel = ViewModel };
        }
    }
}
