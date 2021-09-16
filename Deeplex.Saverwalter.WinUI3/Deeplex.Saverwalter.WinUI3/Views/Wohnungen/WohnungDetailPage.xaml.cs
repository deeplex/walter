using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.ViewModels.Rechnungen;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
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
                ViewModel = new WohnungDetailViewModel(wohnung, App.ViewModel);
            }
            else if (e.Parameter is null) // New Wohnung
            {
                ViewModel = new WohnungDetailViewModel(App.ViewModel);
            }

            AddZaehler_Click = () =>
            {
                var vm = new ZaehlerDetailViewModel(new Zaehler() { WohnungId = ViewModel.Id }, App.ViewModel);
                App.ViewModel.Navigate(typeof(ZaehlerDetailPage), vm);
            };

            AddVertrag_Click = () =>
            {
                var vm = new VertragDetailViewModel(App.ViewModel);
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
                var vm = new BetriebskostenrechnungDetailViewModel(r, App.ViewModel);
                App.ViewModel.Navigate(typeof(BetriebskostenrechnungenDetailPage), vm);
            };

            AddErhaltungsaufwendung_Click = () =>
            {
                var r = new Model.Erhaltungsaufwendung()
                {
                    Wohnung = ViewModel.Entity,
                    Datum = DateTime.Now,
                };

                var vm = new ErhaltungsaufwendungenDetailViewModel(r, App.ViewModel);
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
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.ViewModel));

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

        private void Erhaltungsaufwendung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //var Jahr = (int)((Button)sender).CommandParameter;
            //var l = new ErhaltungsaufwendungWohnung(App.Walter, ViewModel.Id, Jahr);

            //var s = Jahr.ToString() + " - " + ViewModel.Anschrift;
            //var path = ApplicationData.Current.TemporaryFolder.Path + @"\" + s;

            //var worked = l.SaveAsDocx(path + ".docx");
            //var text = worked ? "Datei gespeichert als: " + s : "Datei konnte nicht gespeichert werden.";

            //var anhang = Saverwalter.ViewModels.Utils.Files.ExtractFrom(path + ".docx");

            //if (anhang != null)
            //{
            //    App.Walter.WohnungAnhaenge.Add(new WohnungAnhang()
            //    {
            //        Anhang = anhang,
            //        Target = ViewModel.Entity,
            //    });
            //    App.SaveWalter();
            //    App.ViewModel.DetailAnhang.Value.AddAnhangToList(anhang);
            //    App.ViewModel.ShowAlert(text, 5000);
            //}
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
                Anschrift = AdresseViewModel.Anschrift(id, App.ViewModel);
                Content = Anschrift;
            }
        }
    }
}
