using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class ErhaltungsaufwendungListControl : UserControl
    {
        public ErhaltungsaufwendungListControl()
        {
            InitializeComponent();
            ViewModel = App.Container.GetInstance<ErhaltungsaufwendungListViewModel>();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigate.Execute(ViewModel.Selected.Entity);
        }

        public ErhaltungsaufwendungListViewModel ViewModel
        {
            get { return (ErhaltungsaufwendungListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                "ViewModel",
                typeof(ErhaltungsaufwendungListViewModel),
                typeof(ErhaltungsaufwendungListControl),
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
                  typeof(ErhaltungsaufwendungListControl),
                  new PropertyMetadata(0));

        public ImmutableList<ErhaltungsaufwendungListViewModelEntry> Liste
        {
            get { return (ImmutableList<ErhaltungsaufwendungListViewModelEntry>)GetValue(ListeProperty); }
            set { SetValue(ListeProperty, value); }
        }

        public static readonly DependencyProperty ListeProperty
            = DependencyProperty.Register(
                  "Liste",
                  typeof(ImmutableList<ErhaltungsaufwendungListViewModelEntry>),
                  typeof(ErhaltungsaufwendungListControl),
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
                  typeof(ErhaltungsaufwendungListControl),
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
                  typeof(ErhaltungsaufwendungListControl),
                  new PropertyMetadata(false));


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Window.ListAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            if (((DataGrid)sender).SelectedItem is ErhaltungsaufwendungListViewModelEntry a)
            {
                App.Window.ListAnhang.Value.SetList(a.Entity);
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.List.Value = (sender as DataGrid).Sort(e.Column, ViewModel.List.Value);
        }
    }
}
