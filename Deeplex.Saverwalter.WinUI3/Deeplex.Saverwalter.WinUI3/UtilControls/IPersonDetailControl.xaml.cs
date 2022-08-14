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

        public ObservableProperty<VertragListViewModel> VertragListViewModel { get; set; } = new();
        public ObservableProperty<WohnungListViewModel> WohnungListViewModel { get; set; } = new();
        public ObservableProperty<KontaktListViewModel> JuristischePersonenViewModel { get; set; } = new();

        public IPersonDetailViewModel ViewModel
        {
            get { return (IPersonDetailViewModel)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                VertragListViewModel.Value = App.Container.GetInstance<VertragListViewModel>();
                VertragListViewModel.Value.SetList(ViewModel.Entity);
                WohnungListViewModel.Value = App.Container.GetInstance<WohnungListViewModel>();
                WohnungListViewModel.Value.SetList(ViewModel.Entity);
                // Should only show juristische Person? TODO
                JuristischePersonenViewModel.Value = App.Container.GetInstance<KontaktListViewModel>();
                JuristischePersonenViewModel.Value.SetList(ViewModel.Entity);
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
