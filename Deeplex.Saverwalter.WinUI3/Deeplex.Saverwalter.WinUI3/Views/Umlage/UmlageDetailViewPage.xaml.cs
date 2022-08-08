using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class UmlageDetailViewPage : Page
    {
        public UmlageDetailViewModel ViewModel { get; private set; }
        public WohnungListViewModel WohnungListViewModel { get; private set; }
        public BetriebskostenRechnungenListViewModel BetriebskostenRechnungenListViewModel { get; private set; }

        public UmlageDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.SaveWohnungen();
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Umlage r)
            {
                ViewModel = App.Container.GetInstance<UmlageDetailViewModel>();
                ViewModel.SetEntity(r);
                
                WohnungListViewModel = App.Container.GetInstance<WohnungListViewModel>();
                WohnungListViewModel.SetList(r);

                BetriebskostenRechnungenListViewModel = App.Container.GetInstance<BetriebskostenRechnungenListViewModel>();
                BetriebskostenRechnungenListViewModel.SetList(r);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new(App.NotificationService, App.WalterService);
            }

            base.OnNavigatedTo(e);

            App.Window.CommandBar.MainContent = new DetailCommandBarControl<Umlage>() { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.WalterService));

            App.WalterService.ctx.Adressen
                .Include(i => i.Wohnungen)
                .ThenInclude(i => i.Umlagen)
                .ThenInclude(w => w.Betriebskostenrechnungen)
                .ToList()
                .GroupBy(a => a.Stadt)
                .ToList()
                .ForEach(a =>
                {
                    var ls = a.ToList();
                    if (ls.Count == 0) return;

                    var r = new TreeViewNode() { Content = a.Key };

                    ls.ForEach(d =>
                    {
                        if (d.Wohnungen.Count == 0)
                        {
                            return;
                        }

                        var k = new TreeViewNode() { Content = AdresseViewModel.Anschrift(d) };
                        d.Wohnungen.ForEach(w =>
                        {
                            var n = new TreeViewNode() { Content = new WohnungListViewModelEntry(w, App.WalterService) };
                            k.Children.Add(n);
                            if (ViewModel.Wohnungen.Value.Exists(i => i.Id == w.WohnungId))
                            {
                                WohnungenTree.SelectedNodes.Add(n);
                            }
                        });
                        r.Children.Add(k);
                    });
                    WohnungenTree.RootNodes.Add(r);
                });
        }

        private void WohnungenTree_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var selected = WohnungenTree.SelectedItems
                .Select(s => (s as TreeViewNode).Content)
                .Where(s => s is WohnungListViewModelEntry)
                .Select(s => (WohnungListViewModelEntry)s)
                .ToImmutableList();

            ViewModel.UpdateWohnungen(selected);
        }
    }
}
