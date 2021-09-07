using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.ViewModels.Rechnungen;
using Deeplex.Saverwalter.App.Views.Rechnungen;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Deeplex.Saverwalter.App.Utils.Elements;

namespace Deeplex.Saverwalter.App.UserControls
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
        }

        public ErhaltungsaufwendungenListControl()
        {
            InitializeComponent();
            ViewModel = new ErhaltungsaufwendungenListViewModel(App.ViewModel);

            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, IdProp) => UpdateFilter());
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedAufwendung != null)
            {
                App.ViewModel.Navigate(typeof(ErhaltungsaufwendungenDetailPage), ViewModel.SelectedAufwendung.Id);
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
            App.ViewModel.updateListAnhang(new AnhangListViewModel(a, App.ViewModel));
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Liste.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Liste.Value);
        }
    }
}
