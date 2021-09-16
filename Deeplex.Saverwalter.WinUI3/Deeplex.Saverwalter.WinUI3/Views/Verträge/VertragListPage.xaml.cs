﻿using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Utils;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class VertragListPage : Page
    {
        public VertragListViewModel ViewModel = new VertragListViewModel(App.ViewModel);
        public ObservableProperty<bool> OnlyActive = new ObservableProperty<bool>();

        public VertragListPage()
        {
            InitializeComponent();

            App.ViewModel.Titel.Value = "Verträge";
            var AddVertrag = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Vertrag hinzufügen"
            };
            AddVertrag.Click += AddVertrag_Click;

            App.ViewModel.RefillCommandContainer(
                new ICommandBarElement[] {
                    Elements.CheckBox(OnlyActive, "Nur Laufend"),
                    Elements.Filter(ViewModel),
                    AddVertrag });
        }

        private void AddVertrag_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VertragDetailViewPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
