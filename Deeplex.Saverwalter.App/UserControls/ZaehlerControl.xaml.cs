using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Deeplex.Saverwalter.App.UserControls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ZaehlerControl : Page
    {
        public ZaehlerViewModel ViewModel { get; set; }

        public ZaehlerControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(IdProperty, (idDepObject, idProp) =>
            {
                ViewModel = new ZaehlerViewModel(App.Walter.ZaehlerSet.Find(Id));
            });
        }

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty
            = DependencyProperty.Register(
                "Id",
                typeof(int),
                typeof(ZaehlerControl),
                new PropertyMetadata(0));
    }
}
