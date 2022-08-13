using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ErhaltungsaufwendungWohnungenControl : UserControl
    {

        public ErhaltungsaufwendungWohnungenControl()
        {
            InitializeComponent();
        }

        public ErhaltungsaufwendungPrintViewModel ViewModel
        {
            get { return (ErhaltungsaufwendungPrintViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(ErhaltungsaufwendungPrintViewModel),
            typeof(ErhaltungsaufwendungWohnungenControl),
            new PropertyMetadata(null));

    }
}
