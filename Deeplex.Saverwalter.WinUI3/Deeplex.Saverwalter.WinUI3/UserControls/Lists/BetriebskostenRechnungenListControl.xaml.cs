using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class BetriebskostenRechnungenListControl : UserControl
    {
        public BetriebskostenRechnungenListViewModel ViewModel { get; set; }
        public ObservableProperty<ImmutableList<BetriebskostenRechnungenListEntry>> Templates = new();

        public BetriebskostenRechnungenListControl()
        {
            InitializeComponent();
            ViewModel = new BetriebskostenRechnungenListViewModel(App.WalterService, App.NotificationService);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Selected != null)
            {
                if (WohnungId != 0)
                {
                    if (ViewModel.Selected.Tmpl != 0)
                    {
                        var Wohnungen = App.WalterService.ctx.Betriebskostenrechnungen.Find(ViewModel.Selected.Tmpl).Wohnungen.ToList();
                        App.Window.Navigate(typeof(BetriebskostenrechnungenDetailViewPage), new Tuple<Betriebskostenrechnung, int, List<Wohnung>>(ViewModel.Selected.Entity, WohnungId, Wohnungen));
                    }
                    else
                    {
                        App.Window.Navigate(typeof(BetriebskostenrechnungenDetailViewPage), new Tuple<Betriebskostenrechnung, int>(ViewModel.Selected.Entity, WohnungId));
                    }
                }
                else
                {
                    App.Window.Navigate(typeof(BetriebskostenrechnungenDetailViewPage), ViewModel.Selected.Entity);
                }
            }
        }

        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));

        public int StartJahr
        {
            get { return (int)GetValue(StartJahrProperty); }
            set { SetValue(StartJahrProperty, value); }
        }

        public static readonly DependencyProperty StartJahrProperty
            = DependencyProperty.Register(
                  "StartJahr",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));

        public int EndeJahr
        {
            get { return (int)GetValue(EndeJahrProperty); }
            set { SetValue(EndeJahrProperty, value); }
        }

        public static readonly DependencyProperty EndeJahrProperty
            = DependencyProperty.Register(
                  "EndeJahr",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));

        public int BetreffendesJahr
        {
            get { return (int)GetValue(BetreffendesJahrProperty); }
            set { SetValue(BetreffendesJahrProperty, value); }
        }

        public static readonly DependencyProperty BetreffendesJahrProperty
            = DependencyProperty.Register(
                  "BetreffendesJahr",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));

        public bool ZeigeVorlagen
        {
            get { return (bool)GetValue(ZeigeVorlagenProperty); }
            set { SetValue(ZeigeVorlagenProperty, value); }
        }

        public static readonly DependencyProperty ZeigeVorlagenProperty
            = DependencyProperty.Register(
                "ZeigeVorlagen",
                typeof(bool),
                typeof(BetriebskostenRechnungenListControl),
                new PropertyMetadata(false));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender)?.SelectedItem is BetriebskostenRechnungenListEntry r)
            {
                App.Window.ListAnhang.Value = new AnhangListViewModel(r.Entity, App.FileService, App.NotificationService, App.WalterService);
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.List.Value = (sender as DataGrid).Sort(e.Column, ViewModel.List.Value);
        }
    }
}
