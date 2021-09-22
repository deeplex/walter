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
    public sealed partial class ErhaltungsaufwendungenPrintPage : Page
    {
        public ErhaltungsaufwendungenPrintViewModel ViewModel { get; set; }

        public ErhaltungsaufwendungenPrintPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Wohnung w)
            {
                ViewModel = new ErhaltungsaufwendungenPrintViewModel(w);
            }
            else if (e.Parameter is IPerson p)
            {
                ViewModel = new ErhaltungsaufwendungenPrintViewModel(p, App.ViewModel.ctx);
            }

            App.Window.CommandBar.MainContent = new ErhaltungsaufwendungenPrintCommandBarControl { ViewModel = ViewModel };

            base.OnNavigatedTo(e);
        }
    }
}
