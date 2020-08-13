﻿using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class ZaehlerControl : UserControl
    {
        public ZaehlerViewModel ViewModel { get; set; }

        public ZaehlerControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(IdProperty, (idDepObject, idProp) =>
            {
                ViewModel = Allgemein ?
                    new ZaehlerViewModel(App.Walter.AllgemeinZaehlerSet.Find(Id)) :
                    new ZaehlerViewModel(App.Walter.ZaehlerSet.Find(Id));
            });

            RegisterPropertyChangedCallback(vmProperty, (vmDepObject, vmProp) =>
            {
                ViewModel = vm;
            });
        }

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public bool Allgemein
        {
            get { return (bool)GetValue(AllgemeinProperty); }
            set { SetValue(AllgemeinProperty, value); }
        }

        public static readonly DependencyProperty IdProperty
            = DependencyProperty.Register(
                "Id",
                typeof(int),
                typeof(ZaehlerControl),
                new PropertyMetadata(0));

        public static readonly DependencyProperty AllgemeinProperty
            = DependencyProperty.Register(
                "Allgemein",
                typeof(bool),
                typeof(ZaehlerControl),
                new PropertyMetadata(false));

        public ZaehlerViewModel vm
        {
            get { return (ZaehlerViewModel)GetValue(vmProperty); }
            set { SetValue(vmProperty, value); }
        }

        public static readonly DependencyProperty vmProperty
            = DependencyProperty.Register(
                "vm",
                typeof(ZaehlerViewModel),
                typeof(ZaehlerstandControl),
                new PropertyMetadata(null));
    }
}