using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class WohnungDetailViewPage : Page
    {
        public WohnungDetailViewModel ViewModel { get; set; }
        public VertragListViewModel VertragListViewModel { get; private set; }
        public WohnungListViewModel WohnungAdresseViewModel { get; private set; }
        public BetriebskostenRechnungenListViewModel BetriebskostenrechnungViewModel { get; private set; }
        public UmlageListViewModel UmlageListViewModel { get; private set; }
        public ErhaltungsaufwendungenListViewModel ErhaltungsaufwendungViewModel { get; private set; }
        public ZaehlerListViewModel ZaehlerListViewModel { get; private set; }

        public WohnungDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = App.Container.GetInstance<WohnungDetailViewModel>();
            VertragListViewModel = App.Container.GetInstance<VertragListViewModel>();
            WohnungAdresseViewModel = App.Container.GetInstance<WohnungListViewModel>();
            BetriebskostenrechnungViewModel = App.Container.GetInstance<BetriebskostenRechnungenListViewModel>();
            ErhaltungsaufwendungViewModel = App.Container.GetInstance<ErhaltungsaufwendungenListViewModel>();
            ZaehlerListViewModel = App.Container.GetInstance<ZaehlerListViewModel>();
            UmlageListViewModel = App.Container.GetInstance<UmlageListViewModel>();

            if (e.Parameter is Wohnung wohnung)
            {
                ViewModel.SetEntity(wohnung);
                VertragListViewModel.SetList(wohnung);
                WohnungAdresseViewModel.SetList(ViewModel.Entity.Adresse);
                BetriebskostenrechnungViewModel.SetList(wohnung);
                ErhaltungsaufwendungViewModel.SetList(wohnung);
                ZaehlerListViewModel.SetList(wohnung);
                UmlageListViewModel.SetList(wohnung);
            }
            
            App.Window.CommandBar.MainContent = new DetailCommandBarControl { ViewModel = ViewModel };

            App.Window.DetailAnhang.Value = App.Container.GetInstance<AnhangListViewModel>();
            App.Window.DetailAnhang.Value.SetList(ViewModel.Entity);

            base.OnNavigatedTo(e);
        }
    }
}
