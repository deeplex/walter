using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ErhaltungsaufwendungenListCommandBarControl : UserControl
    {
        public ErhaltungsaufwendungenListCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Erhaltungsaufwendungen";
        }

        public ErhaltungsaufwendungenListViewModel ViewModel
        {
            get { return (ErhaltungsaufwendungenListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(ErhaltungsaufwendungenListViewModel),
            typeof(ErhaltungsaufwendungenListCommandBarControl),
            new PropertyMetadata(null));

        private void AddErhaltungsaufwendung_Click(object sender, RoutedEventArgs e)
        {
            App.Window.AppFrame.Navigate(typeof(ErhaltungsaufwendungenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Filter.Value = ((TextBox)sender).Text;
        }
    }
}
