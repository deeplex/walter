using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
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
                ViewModel.Value = new ZaehlerstandListViewModel(App.Walter.ZaehlerSet.Find(ZaehlerId));
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
                App.ViewModel.ListAnhang.Value = new AnhangListViewModel(m.Entity);
            }
            else
            {
                App.ViewModel.ListAnhang.Value = null;
            }
        }
    }
}
