using Deeplex.Saverwalter.ViewModels.Rechnungen;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
    public sealed partial class ErhaltungsaufwendungenListCommandBarControl : UserControl
    {
        public ErhaltungsaufwendungenListCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Erhaltungsaufwendungen";
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
            typeof(ErhaltungsaufwendungenListCommandBarControl),
            new PropertyMetadata(null));

        private void AddErhaltungsaufwendung_Click(object sender, RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(ErhaltungsaufwendungenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Filter.Value = ((TextBox)sender).Text;
        }
    }
}
