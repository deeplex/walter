using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ErhaltungsaufwendungenListControl : UserControl
    {
        public ErhaltungsaufwendungenListViewModel ViewModel { get; set; }

        private void UpdateFilter()
        {
            ViewModel.Liste.Value = ViewModel.AllRelevant;
            if (Liste != null)
            {
                ViewModel.Liste.Value = Liste;
            }
            if (WohnungId != 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value
                    .Where(v => v.Wohnung.Id == WohnungId)
                    .ToImmutableList();
            }
            if (Filter != "")
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    applyFilter(Filter, v.Aussteller, v.Bezeichnung, v.Wohnung.ToString()))
                    .ToImmutableList();
            }
            if (Jahr != 0)
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value
                    .Where(v => v.Entity.Datum.Year == Jahr)
                    .ToImmutableList();
            }
        }

        public ErhaltungsaufwendungenListControl()
        {
            InitializeComponent();
            ViewModel = new ErhaltungsaufwendungenListViewModel(App.ViewModel);

            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(ListeProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(JahrProperty, (DepObj, IdProp) => UpdateFilter());
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedAufwendung != null)
            {
                App.Window.Navigate(typeof(ErhaltungsaufwendungenDetailPage), ViewModel.SelectedAufwendung.Id);
            }
        }

        public int Jahr
        {
            get { return (int)GetValue(JahrProperty); }
            set { SetValue(JahrProperty, value); }
        }

        public static readonly DependencyProperty JahrProperty
            = DependencyProperty.Register(
                  "Jahr",
                  typeof(int),
                  typeof(ErhaltungsaufwendungenListControl),
                  new PropertyMetadata(0));


        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(ErhaltungsaufwendungenListControl),
                  new PropertyMetadata(0));

        public ImmutableList<ErhaltungsaufwendungenListEntry> Liste
        {
            get { return (ImmutableList<ErhaltungsaufwendungenListEntry>)GetValue(ListeProperty); }
            set { SetValue(ListeProperty, value); }
        }

        public static readonly DependencyProperty ListeProperty
            = DependencyProperty.Register(
                  "Liste",
                  typeof(ImmutableList<ErhaltungsaufwendungenListEntry>),
                  typeof(ErhaltungsaufwendungenListControl),
                  new PropertyMetadata(null));


        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty
            = DependencyProperty.Register(
                  "Filter",
                  typeof(string),
                  typeof(ErhaltungsaufwendungenListControl),
                  new PropertyMetadata(""));

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        public static readonly DependencyProperty EnabledProperty
            = DependencyProperty.Register(
                  "Enabled",
                  typeof(bool),
                  typeof(ErhaltungsaufwendungenListControl),
                  new PropertyMetadata(true));

        public bool Selectable
        {
            get { return (bool)GetValue(SelectableProperty); }
            set { SetValue(SelectableProperty, value); }
        }

        public static readonly DependencyProperty SelectableProperty
            = DependencyProperty.Register(
                  "Selectable",
                  typeof(bool),
                  typeof(ErhaltungsaufwendungenListControl),
                  new PropertyMetadata(false));


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is ErhaltungsaufwendungenListEntry entry)
            {
                var a = entry.Entity;
                App.ViewModel.updateListAnhang(new AnhangListViewModel(a, App.Impl, App.ViewModel));
            }
            else
            {
                App.ViewModel.updateListAnhang(null);
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Liste.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Liste.Value);
        }
    }
}
