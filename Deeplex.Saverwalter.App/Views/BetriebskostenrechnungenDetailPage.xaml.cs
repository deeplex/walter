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
            if (e.Parameter is int id)
            {
                ViewModel = new BetriebskostenrechnungViewModel(App.Walter.Betriebskostenrechnungen.Find(id));
            }
            else if (e.Parameter is null)
            {
                ViewModel = new BetriebskostenrechnungViewModel();
            }

            base.OnNavigatedTo(e);

            App.ViewModel.Titel.Value = ViewModel.Typ.ToDescriptionString();

            var EditToggle = new AppBarToggleButton
            {
                Label = "Bearbeiten",
                Icon = new SymbolIcon(Symbol.Edit),
            };
            EditToggle.Click += EditToggle_Click;

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
            {
                new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Attach),
                    Command = ViewModel.AttachFile,
                }
            }, new ICommandBarElement[] { EditToggle, Delete });



            ViewModel.AdresseGroup.Keys.ToList().ForEach(k =>
            {
                ViewModel.AdresseGroup[k].ForEach(v => k.Children.Add(v));
                WohnungenTree.RootNodes.Add(k);
            });
        }

        private void EditToggle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsInEdit.Value = (sender as AppBarToggleButton).IsChecked ?? false;
        }

        private void SelfDestruct(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.selfDestruct();
            ((Frame)((NavigationView)Frame.Parent).Content).GoBack();
        }
    }
}
