using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class ZaehlerstandListControl : UserControl
    {
        public ObservableProperty<ZaehlerstandListViewModel> ViewModel
            = new ObservableProperty<ZaehlerstandListViewModel>();

        public ZaehlerstandListControl()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(ZaehlerIdProperty, (ZaehlerIdDepObject, ZaehlerIdProp) =>
            {
                ViewModel.Value = new ZaehlerstandListViewModel(App.Walter.ZaehlerSet.Find(ZaehlerId), App.ViewModel);
                ViewModel.Value.Liste.Value = ViewModel.Value.Liste.Value.OrderBy(v => v.Datum).ToImmutableList();
            });

            RegisterPropertyChangedCallback(ZaehlerstandListViewModelProperty, (ZaehlerIdDepObject, ZaehlerIdProp) =>
            {
                ViewModel.Value = ZaehlerstandListViewModel;
                ViewModel.Value.Liste.Value = ViewModel.Value.Liste.Value.OrderBy(v => v.Datum).ToImmutableList();
            });
        }

        public ZaehlerstandListViewModel ZaehlerstandListViewModel
        {
            get { return (ZaehlerstandListViewModel)GetValue(ZaehlerstandListViewModelProperty); }
            set { SetValue(ZaehlerstandListViewModelProperty, value); }
        }

        public static readonly DependencyProperty ZaehlerstandListViewModelProperty
            = DependencyProperty.Register(
            "ZaehlerstandListViewModel",
            typeof(ZaehlerstandListViewModel),
            typeof(ZaehlerstandListControl),
            new PropertyMetadata(null));

        public int ZaehlerId
        {
            get { return (int)GetValue(ZaehlerIdProperty); }
            set { SetValue(ZaehlerIdProperty, value); }
        }

        public static readonly DependencyProperty ZaehlerIdProperty
            = DependencyProperty.Register(
            "ZaehlerId",
            typeof(int),
            typeof(ZaehlerstandListControl),
            new PropertyMetadata(0));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is ZaehlerstandListEntry m)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(m.Entity, App.ViewModel));
            }
            else
            {
                App.ViewModel.clearAnhang();
            }
        }
    }
}
