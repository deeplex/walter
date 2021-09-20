using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.ViewModels.Rechnungen;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{
    public sealed partial class ErhaltungsaufwendungenPrintPage : Page
    {
        public List<ErhaltungsaufwendungenListViewModel> ViewModel { get; set; }

        public ErhaltungsaufwendungenPrintPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is List<ErhaltungsaufwendungenListViewModel> vm)
            {
                ViewModel = vm;
            }
            
            //App.Window.CommandBar.MainContent = new ErhaltungsaufwendungenPrintCommandBarControl { ViewModel = ViewModel };

            base.OnNavigatedTo(e);
        }
    }
}
