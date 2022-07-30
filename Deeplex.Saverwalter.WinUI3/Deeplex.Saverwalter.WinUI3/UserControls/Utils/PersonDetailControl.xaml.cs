using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class PersonDetailControl : UserControl
    {
        public PersonDetailControl()
        {
            InitializeComponent();
        }

        public ObservableProperty<VertragListViewModel> VertragListViewModel { get; set; } = new();
        public ObservableProperty<WohnungListViewModel> WohnungListViewModel { get; set; } = new();
        public ObservableProperty<KontaktListViewModel> JuristischePersonenViewModel { get; set; } = new();

        public PersonViewModel ViewModel
        {
            get { return (PersonViewModel)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                VertragListViewModel.Value = new VertragListViewModel(
                    App.WalterService,
                    App.NotificationService,
                    ViewModel.Entity);
                WohnungListViewModel.Value = new WohnungListViewModel(
                    App.WalterService,
                    App.NotificationService,
                    ViewModel.Entity);
                JuristischePersonenViewModel.Value = new KontaktListViewModel(
                    App.WalterService,
                    App.NotificationService,
                    ViewModel.Entity);
            }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "PersonViewModel",
            typeof(PersonViewModel),
            typeof(PersonDetailControl),
            new PropertyMetadata(null));


        private void Erhaltungsaufwendung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(
                typeof(ErhaltungsaufwendungenPrintViewPage),
                ViewModel.Entity,
                new DrillInNavigationTransitionInfo());
        }

    }
}
