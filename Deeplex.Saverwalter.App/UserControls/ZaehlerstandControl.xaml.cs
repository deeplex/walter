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

            RegisterPropertyChangedCallback(vmProperty, (vmDepObject, vmProp) =>
            {
                ViewModel = vm;
                // TODO this should not be necessary, but without it the values are not set in GUI
                Zaehlerstand.Value = ViewModel.Stand;
                Ablesedatum.Date = ViewModel.Datum;
                Attach.Command = ViewModel.AttachFile;
                Notiz.Text = ViewModel.Notiz ?? "";
                SelfDestruct.Command = ViewModel.SelfDestruct;
            });
        }

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
