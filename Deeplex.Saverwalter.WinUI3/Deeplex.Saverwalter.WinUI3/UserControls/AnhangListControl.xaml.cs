using Deeplex.Saverwalter.WinUI3.Utils;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using System;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class AnhangListControl : UserControl
    {
        public ObservableProperty<AnhangListViewModel> ViewModel
            = new ObservableProperty<AnhangListViewModel>();
        public ObservableProperty<bool> active = new ObservableProperty<bool>();

        public AnhangListControl()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(AnhangListViewModelProperty, (DepObj, Prop) =>
            {
                ViewModel.Value = AnhangViewModel;
                active.Value = ViewModel.Value != null && ViewModel.Value?.Text.Value != "";
            });
        }

        public AnhangListViewModel AnhangViewModel
        {
            get { return (AnhangListViewModel)GetValue(AnhangListViewModelProperty); }
            set { SetValue(AnhangListViewModelProperty, value); }
        }

        public static readonly DependencyProperty AnhangListViewModelProperty
            = DependencyProperty.Register(
            "AnhangListViewModel",
            typeof(AnhangListViewModel),
            typeof(AnhangListControl),
            new PropertyMetadata(null));

        public bool List
        {
            get { return (bool)GetValue(ListProperty); }
            set { SetValue(ListProperty, value); }
        }

        public static readonly DependencyProperty ListProperty
            = DependencyProperty.Register(
            "List",
            typeof(bool),
            typeof(AnhangListControl),
            new PropertyMetadata(false));

        private async void Runterladen_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Loeschen_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is AnhangListEntry a)
            {
                a.DeleteFile();
            }
        }

        private void Oeffne_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is AnhangListEntry a)
            {
                a.OpenFile();
            }
        }
    }
}
