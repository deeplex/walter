using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Utils.ObjectModel;
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


namespace Deeplex.Saverwalter.App.Views
{

    public sealed partial class BetriebskostenRechnungenViewPage : Page
    {
        public BetriebskostenRechnungenViewModel ViewModel = new BetriebskostenRechnungenViewModel();
        public BetriebskostenRechnungenViewPage()
        {
            InitializeComponent();
        }
    }
}
