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

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.Update();
            base.OnNavigatingFrom(e);
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
            App.ViewModel.RefillCommandContainer(new ICommandBarElement[] { },
                new ICommandBarElement[] { Delete });
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
