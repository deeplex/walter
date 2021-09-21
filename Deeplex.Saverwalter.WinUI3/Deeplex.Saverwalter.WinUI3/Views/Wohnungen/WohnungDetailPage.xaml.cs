using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class WohnungDetailPage : Page
    {
        public WohnungDetailViewModel ViewModel { get; set; }

        public WohnungDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Wohnung wohnung)
            {
                ViewModel = new WohnungDetailViewModel(wohnung, App.Impl, App.ViewModel);
            }
            else if (e.Parameter is null) // New Wohnung
            {
                ViewModel = new WohnungDetailViewModel(App.Impl, App.ViewModel);
            }

            AddZaehler_Click = () =>
            {
                var vm = new ZaehlerDetailViewModel(new Zaehler() { WohnungId = ViewModel.Id }, App.Impl, App.ViewModel);
                App.Window.Navigate(typeof(ZaehlerDetailPage), vm);
            };

            AddVertrag_Click = () =>
            {
                var vm = new VertragDetailViewModel(App.Impl, App.ViewModel);
                vm.Wohnung = vm.AlleWohnungen.First(v => v.Entity.WohnungId == ViewModel.Id);
                App.Window.Navigate(typeof(VertragDetailViewPage), vm);
            };

            AddBetriebskostenrechnung_Click = () =>
            {
                var r = new Betriebskostenrechnung()
                {
                    BetreffendesJahr = DateTime.Now.Year,
                    Datum = DateTime.Now,
                };
                r.Gruppen.Add(new BetriebskostenrechnungsGruppe()
                {
                    Wohnung = ViewModel.Entity,
                    Rechnung = r,
                });
                var vm = new BetriebskostenrechnungDetailViewModel(r, App.Impl, App.ViewModel);
                App.Window.Navigate(typeof(BetriebskostenrechnungenDetailPage), vm);
            };

            AddErhaltungsaufwendung_Click = () =>
            {
                var r = new Erhaltungsaufwendung()
                {
                    Wohnung = ViewModel.Entity,
                    Datum = DateTime.Now,
                };

                var vm = new ErhaltungsaufwendungenDetailViewModel(r, App.Impl, App.ViewModel);
                App.Window.Navigate(typeof(ErhaltungsaufwendungenDetailPage), vm);
            };

            App.Window.CommandBar.MainContent = new WohnungDetailCommandBarControl { ViewModel = ViewModel };
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }

        public Action AddZaehler_Click;
        public Action AddVertrag_Click;
        public Action AddBetriebskostenrechnung_Click;
        public Action AddErhaltungsaufwendung_Click;

        public sealed class WohnungDetailAdresseWohnung : TreeViewNode
        {
            public int Id { get; }
            public int AdresseId { get; }
            public string Bezeichnung { get; }

            public WohnungDetailAdresseWohnung(Wohnung w)
            {
                Id = w.WohnungId;
                AdresseId = w.AdresseId;
                Bezeichnung = w.Bezeichnung;
                Content = Bezeichnung;
            }
        }

        private sealed class WohnungDetailAdresse : TreeViewNode
        {
            public int Id { get; }
            public string Anschrift { get; }

            public WohnungDetailAdresse(int id)
            {
                Id = id;
                Anschrift = AdresseViewModel.Anschrift(id, App.ViewModel);
                Content = Anschrift;
            }
        }
    }
}
