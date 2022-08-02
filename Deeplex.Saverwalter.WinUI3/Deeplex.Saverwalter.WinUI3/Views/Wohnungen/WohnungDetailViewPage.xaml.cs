﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class WohnungDetailViewPage : Page
    {
        public WohnungDetailViewModel ViewModel { get; set; }
        public VertragListViewModel VertragListViewModel { get; private set; }
        public WohnungListViewModel WohnungAdresseViewModel { get; private set; }
        public BetriebskostenRechnungenListViewModel BetriebskostenrechnungViewModel { get; private set; }
        public ErhaltungsaufwendungenListViewModel ErhaltungsaufwendungViewModel { get; private set; }
        public ZaehlerListViewModel ZaehlerListViewModel { get; private set; }

        public WohnungDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Wohnung wohnung)
            {
                ViewModel = new(wohnung, App.NotificationService, App.WalterService);
                VertragListViewModel = new(App.WalterService, App.NotificationService, wohnung);
                WohnungAdresseViewModel = new(App.WalterService, App.NotificationService, wohnung.Adresse);
                BetriebskostenrechnungViewModel = new(App.WalterService, App.NotificationService, wohnung);
                ErhaltungsaufwendungViewModel = new(App.WalterService, App.NotificationService, wohnung);
                ZaehlerListViewModel = new(App.WalterService, App.NotificationService, wohnung);
            }
            else if (e.Parameter is null) // New Wohnung
            {
                ViewModel = new WohnungDetailViewModel(App.NotificationService, App.WalterService);
            }

            AddZaehler_Click = () =>
            {
                var vm = new ZaehlerDetailViewModel(new Zaehler() { WohnungId = ViewModel.Id }, App.NotificationService, App.WalterService);
                App.Window.Navigate(typeof(ZaehlerDetailViewPage), vm);
            };

            AddVertrag_Click = () =>
            {
                var vm = new VertragDetailViewModel(App.NotificationService, App.WalterService);
                vm.Wohnung.Value = vm.AlleWohnungen.First(v => v.Entity.WohnungId == ViewModel.Id);
                App.Window.Navigate(typeof(VertragDetailViewPage), vm);
            };

            AddErhaltungsaufwendung_Click = () =>
            {
                var r = new Erhaltungsaufwendung()
                {
                    Wohnung = ViewModel.Entity,
                    Datum = DateTime.Now,
                };

                var vm = new ErhaltungsaufwendungenDetailViewModel(r, App.NotificationService, App.WalterService);
                App.Window.Navigate(typeof(ErhaltungsaufwendungenDetailViewPage), vm);
            };

            App.Window.CommandBar.MainContent = new DetailCommandBarControl { ViewModel = ViewModel };

            App.Window.DetailAnhang.Value = new AnhangListViewModel(ViewModel.Entity, App.FileService, App.NotificationService, App.WalterService);

            base.OnNavigatedTo(e);
        }

        public Action AddZaehler_Click;
        public Action AddVertrag_Click;
        public Action AddBetriebskostenrechnung_Click;
        public Action AddErhaltungsaufwendung_Click;

        public sealed class WohnungDetailAdresseWohnung : TreeViewNode
        {
            public int Id { get; }
            public int AdresseId { get; }
            public string Bezeichnung { get; }

            public WohnungDetailAdresseWohnung(Wohnung w)
            {
                Id = w.WohnungId;
                AdresseId = w.AdresseId;
                Bezeichnung = w.Bezeichnung;
                Content = Bezeichnung;
            }
        }

        private sealed class WohnungDetailAdresse : TreeViewNode
        {
            public int Id { get; }
            public string Anschrift { get; }

            public WohnungDetailAdresse(int id)
            {
                Id = id;
                Anschrift = AdresseViewModel.Anschrift(id, App.WalterService);
                Content = Anschrift;
            }
        }

        private void Erhaltungsaufwendung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(
                typeof(ErhaltungsaufwendungenPrintViewPage),
                ViewModel.Entity,
                new DrillInNavigationTransitionInfo());
        }
    }
}
