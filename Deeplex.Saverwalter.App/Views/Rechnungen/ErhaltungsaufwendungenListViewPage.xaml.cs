using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.App.ViewModels.Rechnungen;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views.Rechnungen
{

    public sealed partial class ErhaltungsaufwendungenListViewPage : Page
    {
        public ErhaltungsaufwendungenListViewModel ViewModel = new ErhaltungsaufwendungenListViewModel();

        public ErhaltungsaufwendungenListViewPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Erhaltungsaufwendungen";

            var AddErhaltungsaufwendung = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Erhaltungsaufwendung hinzufügen",
            };
            AddErhaltungsaufwendung.Click += AddErhaltungsaufwendung_Click;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                Elements.Filter(ViewModel),
                AddErhaltungsaufwendung,
            });
        }

        private void AddErhaltungsaufwendung_Click(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(BetriebskostenrechnungenDetailPage), null,
            //    new DrillInNavigationTransitionInfo());
        }
    }
}
