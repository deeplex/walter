using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class BetriebskostenRechnungenListControl : UserControl
    {
        public BetriebskostenRechnungenListViewModel ViewModel { get; set; }

        public BetriebskostenRechnungenListControl()
        {
            InitializeComponent();
            ViewModel = new BetriebskostenRechnungenListViewModel();

            RegisterPropertyChangedCallback(WohnungIdProperty, (WohnungIdDepObject, WohnungIdProp) =>
            {
                ViewModel.Liste.Value = ViewModel.Liste.Value
                    .Where(v => v.Wohnungen.Select(i => i.WohnungId).Contains(WohnungId))
                    .ToList();
            });
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedRechnung != null)
            {
                App.ViewModel.Navigate(typeof(BetriebskostenrechnungenDetailPage), ViewModel.SelectedRechnung.Id);
            }
        }

        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Property1.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(BetriebskostenRechnungenListControl),
                  new PropertyMetadata(0));
    }
}
