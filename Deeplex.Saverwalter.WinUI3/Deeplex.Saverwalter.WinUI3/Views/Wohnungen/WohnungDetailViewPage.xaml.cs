using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class WohnungDetailViewPage : Page
    {
        public WohnungDetailViewModel ViewModel { get; } = App.Container.GetInstance<WohnungDetailViewModel>();
        public VertragListViewModel VertragListViewModel { get; } = App.Container.GetInstance<VertragListViewModel>();
        public WohnungListViewModel WohnungAdresseViewModel { get; } = App.Container.GetInstance<WohnungListViewModel>();
        public BetriebskostenRechnungenListViewModel BetriebskostenrechnungViewModel { get; } = App.Container.GetInstance<BetriebskostenRechnungenListViewModel>();
        public UmlageListViewModel UmlageListViewModel { get; } = App.Container.GetInstance<UmlageListViewModel>();
        public ErhaltungsaufwendungenListViewModel ErhaltungsaufwendungViewModel { get; } = App.Container.GetInstance<ErhaltungsaufwendungenListViewModel>();
        public ZaehlerListViewModel ZaehlerListViewModel { get; } = App.Container.GetInstance<ZaehlerListViewModel>();

        public WohnungDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
