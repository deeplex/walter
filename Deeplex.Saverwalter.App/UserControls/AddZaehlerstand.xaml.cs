using System;
using System.Collections.Generic;
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
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.App.ViewModels;

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class AddZaehlerstand : UserControl
    {
        public ZaehlerstandDetailViewModel ViewModel = new ZaehlerstandDetailViewModel();
        public DateTimeOffset Datum
        {
            get => ViewModel.Datum;
            set
            {
                ViewModel.Datum = value.DateTime;
            }
        }

        public AddZaehlerstand()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(ZaehlerViewModelProperty, (ZaehlerIdDepObject, ZaehlerIdProp) =>
            {
                ViewModel.Zaehler = ZaehlerViewModel.Entity;
                ViewModel.Stand = ViewModel.Zaehler.Staende.OrderBy(e => e.Datum).Last().Stand;
                ViewModel.Datum = DateTime.Today;
            });

            RegisterPropertyChangedCallback(ZaehlerstandListViewModelProperty, (ZaehlerIdDepObject, ZaehlerIdProp) =>
            {
                ViewModel.Zaehler = App.Walter.ZaehlerSet.Find(ZaehlerstandListViewModel.ZaehlerId);
                var Last = ViewModel.Zaehler?.Staende.OrderBy(e => e.Datum).LastOrDefault();
                if (Last == null)
                {
                    ViewModel.Stand = 0;
                }
                else
                {
                    ViewModel.Stand = Last.Stand;
                }
                ViewModel.Datum = DateTime.Today;
            });
        }

        private void AddZaehlerstand_Click(object sender, RoutedEventArgs e)
        {
            App.Walter.Zaehlerstaende.Add(ViewModel.Entity);
            App.SaveWalter();
            ZaehlerstandListViewModel.AddToList(ViewModel.Entity);
        }

        public ZaehlerDetailViewModel ZaehlerViewModel
        {
            get { return (ZaehlerDetailViewModel)GetValue(ZaehlerViewModelProperty); }
            set { SetValue(ZaehlerViewModelProperty, value); }
        }

        public static readonly DependencyProperty ZaehlerViewModelProperty
            = DependencyProperty.Register(
            "ZaehlerViewModel",
            typeof(ZaehlerDetailViewModel),
            typeof(ZaehlerstandListControl),
            new PropertyMetadata(null));

        public ZaehlerstandListViewModel ZaehlerstandListViewModel
        {
            get { return (ZaehlerstandListViewModel)GetValue(ZaehlerstandListViewModelProperty); }
            set { SetValue(ZaehlerstandListViewModelProperty, value); }
        }

        public static readonly DependencyProperty ZaehlerstandListViewModelProperty
            = DependencyProperty.Register(
            "ZaehlerstandListViewModel",
            typeof(ZaehlerstandListViewModel),
            typeof(ZaehlerstandListControl),
            new PropertyMetadata(null));
    }
}
