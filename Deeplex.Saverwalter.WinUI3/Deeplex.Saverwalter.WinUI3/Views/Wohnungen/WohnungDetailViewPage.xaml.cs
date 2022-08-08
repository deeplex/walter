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
            if (e.Parameter is Wohnung wohnung)
            {
                ViewModel = App.Container.GetInstance<WohnungDetailViewModel>();
                ViewModel.SetEntity(wohnung);

                VertragListViewModel = App.Container.GetInstance<VertragListViewModel>();
                VertragListViewModel.SetList(wohnung);
                
                WohnungAdresseViewModel = App.Container.GetInstance<WohnungListViewModel>();
                WohnungAdresseViewModel.SetList(ViewModel.Entity.Adresse);
                
                BetriebskostenrechnungViewModel = App.Container.GetInstance<BetriebskostenRechnungenListViewModel>();
                BetriebskostenrechnungViewModel.SetList(wohnung);
                
                ErhaltungsaufwendungViewModel = App.Container.GetInstance<ErhaltungsaufwendungenListViewModel>();
                ErhaltungsaufwendungViewModel.SetList(wohnung);

                ZaehlerListViewModel = App.Container.GetInstance<ZaehlerListViewModel>();
                ZaehlerListViewModel.SetList(wohnung);

                UmlageListViewModel = App.Container.GetInstance<UmlageListViewModel>();
                UmlageListViewModel.SetList(wohnung);
            }
            else if (e.Parameter is null) // New Wohnung
            {
                ViewModel = new WohnungDetailViewModel(App.NotificationService, App.WalterService);
            }

            App.Window.CommandBar.MainContent = new DetailCommandBarControl<Wohnung> { ViewModel = ViewModel };

            App.Window.DetailAnhang.Value = new AnhangListViewModel(ViewModel.Entity, App.FileService, App.NotificationService, App.WalterService);

            base.OnNavigatedTo(e);
        }
    }
}
