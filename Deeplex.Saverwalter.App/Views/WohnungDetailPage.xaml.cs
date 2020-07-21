using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;
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

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            if (ViewModel.Besitzer != null) // TODO this is an ugly way to accomplish initial loading of Besitzer in GUI
            {
                BesitzerCombobox.SelectedIndex = ViewModel.AlleVermieter.FindIndex(v => v.Id == ViewModel.Besitzer.Id);
            }
            base.OnNavigatedTo(e);

            App.ViewModel.Titel.Value = ViewModel.Anschrift + " — " + ViewModel.Bezeichnung;
            var EditToggle = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.Edit),
                Label = "Bearbeiten",
            };
            var Delete = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = "Löschen",
                IsEnabled = false, // TODO
            };
            EditToggle.Click += EditToggle_Click;
            App.ViewModel.RefillCommandContainer(new ICommandBarElement[] { },
                new ICommandBarElement[] { EditToggle, Delete });

            var AdresseGroup = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Select(w => new WohnungDetailAdresseWohnung(w))
                .GroupBy(w => w.AdresseId)
                .ToImmutableDictionary(
                    g => new WohnungDetailAdresse(g.Key),
                    g => g.ToImmutableList());

            AdresseGroup.Keys.ToList().ForEach(k =>
            {
                AdresseGroup[k].ForEach(v => k.Children.Add(v));
                AllgemeinZaehlerTree.RootNodes.Add(k);
            });
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
        
        private void EditToggle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsInEdit.Value = (sender as AppBarToggleButton).IsChecked ?? false;
        }
    }
}
