using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{
    public sealed partial class BetriebskostenrechnungPrintPage : Page
    {
        public BetriebskostenrechnungPrintViewModel ViewModel { get; set; }

        public BetriebskostenrechnungPrintPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Vertrag v)
            {
                ViewModel = new BetriebskostenrechnungPrintViewModel(v, App.ViewModel, App.Impl);
            }

            App.Window.CommandBar.MainContent = new BetriebskostenrechnungPrintCommandBarControl { ViewModel = ViewModel };

            base.OnNavigatedTo(e);
        }
    }
}
