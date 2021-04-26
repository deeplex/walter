using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class ZaehlerDetailPage : Page
    {
        public ZaehlerDetailViewModel ViewModel { get; set; }

        public ZaehlerDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Zaehler zaehler)
            {
                ViewModel = new ZaehlerDetailViewModel(zaehler);
            }
            else if (e.Parameter is null) // New Zaehler
            {
                ViewModel = new ZaehlerDetailViewModel();
            }
        }
    }
}
