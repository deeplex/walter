using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class IPersonDetailControl : UserControl
    {
        public IPersonDetailControl()
        {
            InitializeComponent();
        }

        public VertragListViewModel VertragListViewModel { get; set; } = App.Container.GetInstance<VertragListViewModel>();
        public WohnungListViewModel WohnungListViewModel { get; set; } = App.Container.GetInstance<WohnungListViewModel>();
        public KontaktListViewModel JuristischePersonenViewModel { get; set; } = App.Container.GetInstance<KontaktListViewModel>();
        public MemberViewModel<IPerson> SelectJuristischePersonListViewModel { get; } = App.Container.GetInstance<MemberViewModel<IPerson>>();

        public IPersonDetailViewModel ViewModel
        {
            get { return (IPersonDetailViewModel)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                VertragListViewModel.SetList(ViewModel.Entity);
                WohnungListViewModel.SetList(ViewModel.Entity);
                JuristischePersonenViewModel.SetList(ViewModel.Entity);
                SelectJuristischePersonListViewModel.SetList(ViewModel.Entity, JuristischePersonenViewModel, true);
            }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "PersonViewModel",
            typeof(IPersonDetailViewModel),
            typeof(IPersonDetailControl),
            new PropertyMetadata(null));


        private void Erhaltungsaufwendung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(
                typeof(ErhaltungsaufwendungPrintViewPage),
                ViewModel.Entity,
                new DrillInNavigationTransitionInfo());
        }

    }
}
