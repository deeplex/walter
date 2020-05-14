﻿using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Deeplex.Saverwalter.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class KalteBetriebskostenRechnungPage : Page
    {
        public KalteBetriebskostenRechnungViewModel ViewModel { get; set; }

        public KalteBetriebskostenRechnungPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int adresseId)
            {
                ViewModel = new KalteBetriebskostenRechnungViewModel(adresseId);
            }
            else if (e.Parameter is null) // New Contact
            {
               // ViewModel = new KalteBetriebskostenRechnungViewModel();
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }
    }
}
