﻿using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class WohnungListControl : UserControl
    {
        public WohnungListViewModel ViewModel;

        private void UpdateFilter()
        {
            ViewModel.Liste.Value = ViewModel.AllRelevant;
            if (Filter != "")
            {
                bool low(string str) => str.ToLower().Contains(Filter.ToLower());
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    low(v.Bezeichnung) || low(v.Anschrift))
                    .ToImmutableList();
            }
        }

        public WohnungListControl()
        {
            InitializeComponent();
            ViewModel = new WohnungListViewModel();
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Navigate(typeof(WohnungDetailPage), ViewModel.SelectedWohnung.Value.Entity);
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty
            = DependencyProperty.Register(
                  "Filter",
                  typeof(string),
                  typeof(VertragListControl),
                  new PropertyMetadata(""));
    }
}