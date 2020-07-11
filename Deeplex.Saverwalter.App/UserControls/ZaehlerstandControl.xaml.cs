using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class ZaehlerstandControl : UserControl
    {
        public ZaehlerstandViewModel ViewModel { get; set; }

        public ZaehlerstandControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(IdProperty, (idDepObject, idProp) =>
            {
                ViewModel = new ZaehlerstandViewModel(App.Walter.Zaehlerstaende.Find(Id));
            });

            RegisterPropertyChangedCallback(vmProperty, (vmDepObject, vmProp) =>
            {
                ViewModel = vm;
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
                typeof(ZaehlerstandControl),
                new PropertyMetadata(0));

        public ZaehlerstandViewModel vm
        {
            get { return (ZaehlerstandViewModel)GetValue(vmProperty); }
            set { SetValue(vmProperty, value); }
        }

        public static readonly DependencyProperty vmProperty
            = DependencyProperty.Register(
                "vm",
                typeof(ZaehlerstandViewModel),
                typeof(ZaehlerstandControl),
                new PropertyMetadata(null));
    }
}
