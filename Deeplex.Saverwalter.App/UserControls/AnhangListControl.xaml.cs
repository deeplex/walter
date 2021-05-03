using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private async void ListItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is AnhangListEntry a)
            {
                var path = await a.SaveFile();
                App.ViewModel.ShowAlert("Gespeichert unter " + path, 5000);
                //var path = a.SaveFileTemp();
                //var start = new ProcessStartInfo(path);
                //Process.Start(start);
            }
        }
    }
}
