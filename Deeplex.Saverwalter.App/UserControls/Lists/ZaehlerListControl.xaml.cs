using Deeplex.Saverwalter.App.ViewModels.Zähler;
using Deeplex.Saverwalter.App.Views;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class ZaehlerListControl : UserControl
    {
        public ZaehlerListViewModel ViewModel { get; set; }

        public ZaehlerListControl()
        {
            InitializeComponent();
            ViewModel = new ZaehlerListViewModel();

            RegisterPropertyChangedCallback(WohnungIdProperty, (WohnungIdDepObject, WohnungIdProp) =>
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value.Where(v =>
                    v.WohnungId == WohnungId).ToList();
            });
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedZaehler != null)
            {
                App.ViewModel.Navigate(
                    typeof(ZaehlerDetailPage),
                    App.Walter.ZaehlerSet.Find(ViewModel.SelectedZaehler.Id));
            }
        }

        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(VertragListControl),
                  new PropertyMetadata(0));
    }
}
