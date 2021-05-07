using Deeplex.Saverwalter.App.ViewModels.Rechnungen;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Deeplex.Saverwalter.App.ViewModels;
using static Deeplex.Saverwalter.App.Utils.Elements;
using System.Collections.Immutable;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class ErhaltungsaufwendungenListControl : UserControl
    {
        public ErhaltungsaufwendungenListViewModel ViewModel { get; set; }

        private void UpdateFilter()
        {
            ViewModel.Liste.Value = ViewModel.AllRelevant;
            //if (WohnungId != 0)
            //{
            //    ViewModel.Liste.Value = ViewModel.Liste.Value
            //        .Where(v => v.Wohnungen.Select(i => i.WohnungId).Contains(WohnungId))
            //        .ToImmutableList();
            //}
            if (Filter != "")
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    applyFilter(Filter))
                    .ToImmutableList();
            }
        }

        public ErhaltungsaufwendungenListControl()
        {
            InitializeComponent();
            ViewModel = new ErhaltungsaufwendungenListViewModel();

            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, IdProp) => UpdateFilter());
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, IdProp) => UpdateFilter());
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedAufwendung != null)
            {
                // App.ViewModel.Navigate(typeof(ErhaltungsaufwendungenDetailPage), ViewModel.SelectedAufwendung.Id);
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
            App.ViewModel.ListAnhang.Value = new AnhangListViewModel(a);
        }
    }
}
