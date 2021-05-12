using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Deeplex.Saverwalter.App.Utils.Elements;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class WohnungListControl : UserControl
    {
        public WohnungListViewModel ViewModel;

        private void UpdateFilter()
        {
            if (Liste == null)
            {
                ViewModel.Liste.Value = ViewModel.AllRelevant;
            }
            else
            {
                ViewModel.Liste.Value = Liste;
            }

            if (Filter != "")
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    applyFilter(Filter, v.Bezeichnung, v.Anschrift))
                    .ToImmutableList();
            }
        }

        public WohnungListControl()
        {
            InitializeComponent();
            ViewModel = new WohnungListViewModel();
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(ListeProperty, (DepObj, Prop) => UpdateFilter());
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Navigate(typeof(WohnungDetailPage), ViewModel.SelectedWohnung.Value.Entity);
        }

        public ImmutableList<WohnungListEntry> Liste
        {
            get { return (ImmutableList<WohnungListEntry>)GetValue(ListeProperty); }
            set { SetValue(ListeProperty, value); }
        }

        public static readonly DependencyProperty ListeProperty
            = DependencyProperty.Register(
                  "Liste",
                  typeof(ImmutableList<WohnungListEntry>),
                  typeof(WohnungListControl),
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
                  typeof(WohnungListControl),
                  new PropertyMetadata(""));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((WohnungListEntry)((DataGrid)sender).SelectedItem).Entity;
            App.ViewModel.ListAnhang.Value = new AnhangListViewModel(a);
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Liste.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Liste.Value);
        }
    }
}
