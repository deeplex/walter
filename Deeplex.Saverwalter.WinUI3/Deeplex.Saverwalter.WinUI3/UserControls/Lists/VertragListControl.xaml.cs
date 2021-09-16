using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class VertragListControl : UserControl
    {
        public VertragListViewModel ViewModel { get; set; }

        private void UpdateFilter()
        {
            ViewModel.Vertraege.Value = ViewModel.AllRelevant;
            if (OnlyActive)
            {
                ViewModel.Vertraege.Value = ViewModel.Vertraege.Value.Where(v =>
                !v.hasEnde || v.Ende > DateTime.Now).ToImmutableList();
            }
            if (PersonId != Guid.Empty)
            {
                ViewModel.Vertraege.Value = ViewModel.Vertraege.Value.Where(v =>
                    v.Wohnung.BesitzerId == PersonId ||
                    v.Mieter.Contains(PersonId))
                    .ToImmutableList();
            }
            if (WohnungId != 0)
            {
                ViewModel.Vertraege.Value = ViewModel.Vertraege.Value.Where(v => v.Wohnung.WohnungId == WohnungId).ToImmutableList();
            }
            if (Filter != "")
            {
                ViewModel.Vertraege.Value = ViewModel.Vertraege.Value.Where(v =>
                    applyFilter(Filter, v.Wohnung.Bezeichnung, v.AuflistungMieter, v.Anschrift))
                    .ToImmutableList();
            }
        }

        public VertragListControl()
        {
            InitializeComponent();
            ViewModel = new VertragListViewModel(App.ViewModel);

            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(PersonIdProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(OnlyActiveProperty, (DepObj, Prop) => UpdateFilter());
        }

        public Guid PersonId
        {
            get { return (Guid)GetValue(PersonIdProperty); }
            set { SetValue(PersonIdProperty, value); }
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedVertrag.Value != null)
            {
                App.ViewModel.Navigate(typeof(VertragDetailViewPage), ViewModel.SelectedVertrag.Value.VertragId);
            }
        }

        public static readonly DependencyProperty PersonIdProperty
            = DependencyProperty.Register(
                "PersonId",
                typeof(Guid),
                typeof(VertragListControl),
                new PropertyMetadata(Guid.Empty));

        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(VertragListControl),
                  new PropertyMetadata(0));

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty
            = DependencyProperty.Register(
                  "Filter",
                  typeof(string),
                  typeof(VertragListControl),
                  new PropertyMetadata(""));

        public bool OnlyActive
        {
            get { return (bool)GetValue(OnlyActiveProperty); }
            set { SetValue(OnlyActiveProperty, value); }
        }

        public static readonly DependencyProperty OnlyActiveProperty
            = DependencyProperty.Register(
                  "OnlyActive",
                  typeof(bool),
                  typeof(VertragListControl),
                  new PropertyMetadata(false));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((VertragListVertrag)((DataGrid)sender).SelectedItem).Entity;
            App.ViewModel.updateListAnhang(new AnhangListViewModel(a, App.ViewModel));
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Vertraege.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Vertraege.Value);
        }
    }
}
