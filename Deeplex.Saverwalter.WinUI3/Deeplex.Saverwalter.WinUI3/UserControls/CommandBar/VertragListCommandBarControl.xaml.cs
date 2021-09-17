﻿using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
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
    public sealed partial class VertragListCommandBarControl : UserControl
    {
        public VertragListCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Verträge";
        }

        public VertragListViewModel ViewModel
        {
            get { return (VertragListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(VertragListViewModel),
            typeof(VertragListCommandBarControl),
            new PropertyMetadata(null));

        private void AddVertrag_Click(object sender, RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(VertragDetailViewPage), null,
                new DrillInNavigationTransitionInfo());
        }

        void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Filter.Value = ((TextBox)sender).Text;
        }
    }
}
