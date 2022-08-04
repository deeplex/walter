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
        public ZaehlerstandListControl()
        {
            InitializeComponent();
        }

        public ZaehlerstandListViewModel ViewModel
        {
            get { return (ZaehlerstandListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
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
