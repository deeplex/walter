using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
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
                ViewModel = new WohnungDetailViewModel(wohnung);
            }
            else if (e.Parameter is null) // New Wohnung
            {
                ViewModel = new WohnungDetailViewModel();
            }

            AddZaehler_Click = () =>
            {
                var vm = new ZaehlerDetailViewModel(new Zaehler() { WohnungId = ViewModel.Id });
                App.ViewModel.Navigate(typeof(ZaehlerDetailPage), vm);
            };

            AddVertrag_Click = () =>
            {
                var vm = new VertragDetailViewModel();
                vm.Wohnung = vm.AlleWohnungen.First(v => v.Entity.WohnungId == ViewModel.Id);
                App.ViewModel.Navigate(typeof(VertragDetailViewPage), vm);
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
                var vm = new BetriebskostenrechnungDetailViewModel(r);
                App.ViewModel.Navigate(typeof(BetriebskostenrechnungenDetailPage), vm);
            };


            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            if (ViewModel.Besitzer != null) // TODO this is an ugly way to accomplish initial loading of Besitzer in GUI
            {
                BesitzerCombobox.SelectedIndex = ViewModel.AlleVermieter.FindIndex(v => v.Id == ViewModel.Besitzer.Id);
            }

            App.ViewModel.Titel.Value = ViewModel.Anschrift + " — " + ViewModel.Bezeichnung;
            var Delete = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = "Löschen",
            };
            Delete.Click += Delete_Click;
            App.ViewModel.RefillCommandContainer(new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });

            base.OnNavigatedTo(e);
        }

        public Action AddZaehler_Click;
        public Action AddVertrag_Click;
        public Action AddBetriebskostenrechnung_Click;

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.selfDestruct();
            try
            {
                Frame.GoBack();
            }
            catch
            {
                App.ViewModel.ShowAlert("NAAAA", 2000);
            }
        }

        public sealed class WohnungDetailAdresseWohnung : Microsoft.UI.Xaml.Controls.TreeViewNode
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

        private sealed class WohnungDetailAdresse : Microsoft.UI.Xaml.Controls.TreeViewNode
        {
            public int Id { get; }
            public string Anschrift { get; }

            public WohnungDetailAdresse(int id)
            {
                Id = id;
                Anschrift = AdresseViewModel.Anschrift(id);
                Content = Anschrift;
            }
        }
    }
}
