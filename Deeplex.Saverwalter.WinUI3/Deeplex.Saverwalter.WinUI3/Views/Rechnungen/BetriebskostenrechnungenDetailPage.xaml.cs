using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
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
                ViewModel = new BetriebskostenrechnungDetailViewModel(App.Walter.Betriebskostenrechnungen.Find(id), App.Impl, App.ViewModel);
            }
            else if (e.Parameter is BetriebskostenrechnungDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else if (e.Parameter is Betriebskostenrechnung r)
            {
                ViewModel = new BetriebskostenrechnungDetailViewModel(r, App.Impl, App.ViewModel);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new BetriebskostenrechnungDetailViewModel(App.Impl, App.ViewModel);
            }

            base.OnNavigatedTo(e);

            App.Window.CommandBar.MainContent = new BetriebskostenRechnungenCommandBarControl() { ViewModel = ViewModel };
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            App.Walter.Adressen
                .Include(i => i.Wohnungen)
                .ThenInclude(w => w.Betriebskostenrechnungsgruppen)
                .ToList()
                .ForEach(a =>
                {
                    if (a.Wohnungen.Count == 0) return;

                    var k = new TreeViewNode()
                    {
                        Content = AdresseViewModel.Anschrift(a),
                    };
                    a.Wohnungen.ForEach(w =>
                    {
                        var n = new TreeViewNode() { Content = new WohnungListEntry(w, App.ViewModel) };
                        k.Children.Add(n);
                        if (ViewModel.Wohnungen.Value.Exists(ww => ww.Id == w.WohnungId))
                        {
                            WohnungenTree.SelectedNodes.Add(n);
                        }
                    });
                    WohnungenTree.RootNodes.Add(k);
                });
        }

        private void WohnungenTree_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var selected = WohnungenTree.SelectedItems
                .Select(s => (s as TreeViewNode).Content)
                .Where(s => s is WohnungListEntry)
                .Select(s => (WohnungListEntry)s)
                .ToImmutableList();

            ViewModel.UpdateWohnungen(selected);
        }
    }
}
