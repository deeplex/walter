﻿using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ZaehlerstandListControl : UserControl
    {
        public ObservableProperty<ZaehlerstandListViewModel> ViewModel = new();

        public ZaehlerstandListControl()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(ZaehlerIdProperty, (ZaehlerIdDepObject, ZaehlerIdProp) =>
            {
                ViewModel.Value = new ZaehlerstandListViewModel(App.WalterService.ctx.ZaehlerSet.Find(ZaehlerId), App.NotificationService, App.WalterService);
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
            if (((DataGrid)sender).SelectedItem is ZaehlerstandListViewModelEntry m)
            {
                App.Window.ListAnhang.Value = new AnhangListViewModel(m.Entity, App.FileService, App.NotificationService, App.WalterService);
            }
            else
            {
                App.Window.ListAnhang.Value = null;
            }
        }
    }
}
