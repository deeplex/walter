using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class BetriebskostenrechnungenDetailPage : Page
    {
        public BetriebskostenrechnungViewModel ViewModel { get; private set; }

        public BetriebskostenrechnungenDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Betriebskostenrechnung rechnung)
            {
                ViewModel = new BetriebskostenrechnungViewModel(rechnung);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new BetriebskostenrechnungViewModel();
            }

            App.ViewModel.Titel.Value = "TODO";

            ViewModel.AdresseGroup.Keys.ToList().ForEach(k =>
            {
                ViewModel.AdresseGroup[k].ForEach(v => k.Children.Add(v));
                WohnungenTree.RootNodes.Add(k);
            });

            base.OnNavigatedTo(e);
        }
    }
}
