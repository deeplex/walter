using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ErhaltungsaufwendungenListControl : UserControl
    {
        public ErhaltungsaufwendungenListControl()
        {
            InitializeComponent();
            ViewModel = new ErhaltungsaufwendungenListViewModel(App.WalterService);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Selected != null)
            {
                App.Window.Navigate(typeof(ErhaltungsaufwendungenDetailViewPage), ViewModel.Selected.Id);
            }
        }

        public ErhaltungsaufwendungenListViewModel ViewModel
        {
            get { return (ErhaltungsaufwendungenListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                "ViewModel",
                typeof(ErhaltungsaufwendungenListViewModel),
                typeof(ErhaltungsaufwendungenListControl),
                new PropertyMetadata(null));

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

        public ImmutableList<ErhaltungsaufwendungenListViewModelEntry> Liste
        {
            get { return (ImmutableList<ErhaltungsaufwendungenListViewModelEntry>)GetValue(ListeProperty); }
            set { SetValue(ListeProperty, value); }
        }

        public static readonly DependencyProperty ListeProperty
            = DependencyProperty.Register(
                  "Liste",
                  typeof(ImmutableList<ErhaltungsaufwendungenListViewModelEntry>),
                  typeof(ErhaltungsaufwendungenListControl),
                  new PropertyMetadata(null));

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
            if (((DataGrid)sender).SelectedItem is ErhaltungsaufwendungenListViewModelEntry entry)
            {
                var a = entry.Entity;
                App.Window.ListAnhang.Value = new AnhangListViewModel(a, App.FileService, App.NotificationService, App.WalterService);
            }
            else
            {
                App.Window.ListAnhang.Value = null;
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.List.Value = (sender as DataGrid).Sort(e.Column, ViewModel.List.Value);
        }
    }
}
