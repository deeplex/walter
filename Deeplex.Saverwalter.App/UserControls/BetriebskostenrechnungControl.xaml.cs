using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class BetriebskostenrechnungControl : UserControl
    {
        public BetriebskostenrechnungDetailViewModel ViewModel { get; set; }

        public bool ByGroup => !ByType;

        public BetriebskostenrechnungControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(IdProperty, (idDepObject, idProp) =>
            {
                ViewModel = new BetriebskostenrechnungDetailViewModel(
                    App.Walter.Betriebskostenrechnungen.Find(Id));
            });
        }

        public bool ByType
        {
            get { return (bool)GetValue(ByTypeProperty); }
            set { SetValue(ByTypeProperty, value); }
        }

        public static readonly DependencyProperty ByTypeProperty
            = DependencyProperty.Register(
                "ByType",
                typeof(bool),
                typeof(BetriebskostenrechnungControl),
                new PropertyMetadata(false));

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Property1.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty
            = DependencyProperty.Register(
                  "Id",
                  typeof(int),
                  typeof(BetriebskostenrechnungControl),
                  new PropertyMetadata(0));

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as Button).CommandParameter;
            App.ViewModel.Navigate(typeof(BetriebskostenrechnungenDetailPage), id);
        }
    }
}
