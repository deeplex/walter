using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.ViewModels.Rechnungen;
using Deeplex.Saverwalter.App.Views.Rechnungen;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.ErhaltungsaufwendungListe;
using Deeplex.Saverwalter.Print;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Storage;
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

            AddErhaltungsaufwendung_Click = () =>
            {
                var r = new Model.Erhaltungsaufwendung()
                {
                    Wohnung = ViewModel.Entity,
                    Datum = DateTime.Now,
                };

                var vm = new ErhaltungsaufwendungenDetailViewModel(r);
                App.ViewModel.Navigate(typeof(ErhaltungsaufwendungenDetailPage), vm);
            };

            App.ViewModel.Titel.Value = ViewModel.Anschrift + " — " + ViewModel.Bezeichnung;
            var Delete = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = "Löschen",
            };
            Delete.Click += Delete_Click;
            App.ViewModel.RefillCommandContainer(
                new ICommandBarElement[] { ErhaltungsaufwendungsButton() },
                new ICommandBarElement[] { Delete });
            App.ViewModel.DetailAnhang.Value = new AnhangListViewModel(ViewModel.Entity);

            base.OnNavigatedTo(e);
        }

        public Action AddZaehler_Click;
        public Action AddVertrag_Click;
        public Action AddBetriebskostenrechnung_Click;
        public Action AddErhaltungsaufwendung_Click;

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (await App.ViewModel.Confirmation())
            {
                ViewModel.selfDestruct();
                Frame.GoBack();
            }
        }

        private AppBarButton ErhaltungsaufwendungsButton()
        {
            var ErhAufwButtons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            ErhAufwButtons.Children.Add(new NumberBox()
            {
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                AllowFocusOnInteraction = true,
                Value = ViewModel.ErhaltungsaufwendungJahr.Value,
            });
            var AddErhAufwBtn = new Button
            {
                CommandParameter = ViewModel.ErhaltungsaufwendungJahr.Value,
                Content = new SymbolIcon(Symbol.SaveLocal),
            };
            ErhAufwButtons.Children.Add(AddErhAufwBtn);
            AddErhAufwBtn.Click += Erhaltungsaufwendung_Click;
            return new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.PostUpdate),
                Label = "Erhaltungsaufwendungen",
                Flyout = new Flyout()
                {
                    Content = ErhAufwButtons,
                },
            };
        }

        private void Erhaltungsaufwendung_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var Jahr = (int)((Button)sender).CommandParameter;
            var l = new ErhaltungsaufwendungWohnung(App.Walter, ViewModel.Id, Jahr);

            var s = Jahr.ToString() + " - " + ViewModel.Anschrift;
            var path = ApplicationData.Current.LocalFolder.Path + @"\" + s;

            var worked = l.SaveAsDocx(path + ".docx");
            var text = worked ? "Datei gespeichert als: " + s : "Datei konnte nicht gespeichert werden.";

            // TODO e.SaveAnhaenge(path);

            App.ViewModel.ShowAlert(text, 5000);
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
