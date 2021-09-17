using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ZaehlerCommandBarControl : UserControl
    {
        public ZaehlerCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Zähler"; // TODO Bezeichnung...
        }

        public ZaehlerDetailViewModel ViewModel
        {
            get { return (ZaehlerDetailViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(ZaehlerDetailViewModel),
            typeof(ZaehlerCommandBarControl),
            new PropertyMetadata(null));

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SelfDestruct();
            App.Window.AppFrame.GoBack();
        }
    }
}
