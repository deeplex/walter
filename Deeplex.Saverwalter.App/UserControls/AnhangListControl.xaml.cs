using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.UserControls
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
            if (((FrameworkElement)sender).DataContext is AnhangListEntry a)
            {
                var path = a.SaveFile(); // TODO await
                if (path != "")
                {
                    App.ViewModel.ShowAlert("Gespeichert unter " + path, 5000);
                }
            }
        }

        private void Loeschen_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is AnhangListEntry a)
            {
                a.DeleteFile();
            }
        }

        private async void Oeffne_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is AnhangListEntry a)
            {
                var path = a.SaveFileTemp();
                var file = await StorageFile.GetFileFromPathAsync(path);
                await Windows.System.Launcher.LaunchFileAsync(file);
            }
        }
    }
}
