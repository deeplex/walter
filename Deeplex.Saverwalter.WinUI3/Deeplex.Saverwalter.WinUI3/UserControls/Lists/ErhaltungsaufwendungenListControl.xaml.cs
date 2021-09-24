using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((ErhaltungsaufwendungenListEntry)((DataGrid)sender).SelectedItem).Entity;
            App.ViewModel.updateListAnhang(new AnhangListViewModel(a, App.Impl, App.ViewModel));
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Liste.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Liste.Value);
        }
    }

    public class IsDisabledConverter : IValueConverter
    {
        public Style enabled => App.Window.AppFrame.Resources["AppBarItemForegroundThemeBrush"] as Style;
        public Style disabled => App.Window.AppFrame.Resources["AppBarItemDisabledForegroundThemeBrush"] as Style;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? isDisabled = (bool)value;
            if (isDisabled.HasValue && isDisabled.Value == true)
            {
                return disabled;
            }
            return enabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return enabled;
        }
    }
}
