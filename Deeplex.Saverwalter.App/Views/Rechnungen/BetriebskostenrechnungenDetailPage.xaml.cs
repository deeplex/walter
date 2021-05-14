using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class BetriebskostenrechnungenDetailPage : Page
    {
        public BetriebskostenrechnungDetailViewModel ViewModel { get; private set; }

        public BetriebskostenrechnungenDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            WohnungenTree_Tapped(null, null);
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int id)
            {
                ViewModel = new BetriebskostenrechnungDetailViewModel(App.Walter.Betriebskostenrechnungen.Find(id));
            }
            if (e.Parameter is BetriebskostenrechnungDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else if (e.Parameter is null)
            {
                ViewModel = new BetriebskostenrechnungDetailViewModel();
            }

            base.OnNavigatedTo(e);

            App.ViewModel.Titel.Value = "Betriebskostenrechnung";
            App.ViewModel.DetailAnhang.Value = new AnhangListViewModel(ViewModel.Entity);

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.ViewModel.RefillCommandContainer(
                new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });

            App.Walter.Adressen
                .Include(i => i.Wohnungen)
                .ThenInclude(w => w.Betriebskostenrechnungsgruppen)
                .ToList()
                .ForEach(a =>
                {
                    if (a.Wohnungen.Count == 0) return;

                    var k = new Microsoft.UI.Xaml.Controls.TreeViewNode()
                    {
                        Content = AdresseViewModel.Anschrift(a),
                    };
                    a.Wohnungen.ForEach(w =>
                    {
                        var n = new Microsoft.UI.Xaml.Controls.TreeViewNode() { Content = new WohnungListEntry(w) };
                        k.Children.Add(n);
                        if (ViewModel.Wohnungen.Value.Exists(ww => ww.Id == w.WohnungId))
                        {
                            WohnungenTree.SelectedNodes.Add(n);
                        }
                    });
                    WohnungenTree.RootNodes.Add(k);
                });

            WohnungenTree.Tapped += WohnungenTree_Tapped;
        }

        private void WohnungenTree_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ViewModel.Id == 0) return;
            var flagged = false;

            var selected = WohnungenTree.SelectedItems
                .Select(s => (s as Microsoft.UI.Xaml.Controls.TreeViewNode).Content)
                .Where(s => s is WohnungListEntry)
                .ToList();

            // Add missing Gruppen
            selected
                .Where(s => !ViewModel.Wohnungen.Value.Exists(w => w.Id == (s as WohnungListEntry).Id))
                .ToList()
                .ForEach(s =>
                {
                    flagged = true;

                    App.Walter.Betriebskostenrechnungsgruppen.Add(new BetriebskostenrechnungsGruppe()
                    {
                        Rechnung = App.Walter.Betriebskostenrechnungen.Find(ViewModel.Id),
                        WohnungId = (s as WohnungListEntry).Id,
                    });
                    ViewModel.Wohnungen.Value = ViewModel.Wohnungen.Value.Add(s as WohnungListEntry);
                });

            // Remove old Gruppen
            ViewModel.Wohnungen.Value
                .Where(w => !selected.Exists(s => w.Id == (s as WohnungListEntry).Id))
                .ToList()
                .ForEach(w =>
                {
                    flagged = true;

                    App.Walter.Betriebskostenrechnungsgruppen
                        .Where(g => g.Rechnung.BetriebskostenrechnungId == ViewModel.Id && g.WohnungId == w.Id)
                        .ToList()
                        .ForEach(g =>
                        {
                            App.Walter.Betriebskostenrechnungsgruppen.Remove(g);
                            ViewModel.Wohnungen.Value = ViewModel.Wohnungen.Value.Remove(w);
                        });
                });

            if (flagged) ViewModel.Update();
        }

        private async void SelfDestruct(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (await App.ViewModel.Confirmation())
            {
                if (ViewModel.Id != 0)
                {
                    ViewModel.selfDestruct();
                }
                Frame.GoBack();
            }
        }
    }
}
