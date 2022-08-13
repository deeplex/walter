﻿using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class AnhangListControl : UserControl
    {
        public ObservableProperty<AnhangListViewModel> ViewModel = new();
        public ObservableProperty<bool> active = new();

        public AnhangListControl()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(AnhangListViewModelProperty, (DepObj, Prop) =>
            {
                ViewModel.Value = AnhangViewModel;
                active.Value = ViewModel.Value != null && ViewModel.Value?.ToString() != "";
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
            if (((FrameworkElement)sender).DataContext is AnhangListViewModelEntry a)
            {
                var picker = FileService.FileSavePicker(Path.GetExtension(a.DateiName));
                picker.SuggestedFileName = a.DateiName;
                await picker.PickSaveFileAsync();
            }
        }

        private void Loeschen_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is AnhangListViewModelEntry a)
            {
                a.DeleteFile();
            }
        }

        private void Oeffne_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is AnhangListViewModelEntry a)
            {
                a.OpenFile();
            }
        }
    }
}
