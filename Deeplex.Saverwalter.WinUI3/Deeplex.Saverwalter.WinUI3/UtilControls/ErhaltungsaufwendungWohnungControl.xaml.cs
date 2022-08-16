using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class ErhaltungsaufwendungWohnungControl : UserControl
    {

        public ErhaltungsaufwendungWohnungControl()
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
            typeof(ErhaltungsaufwendungWohnungControl),
            new PropertyMetadata(null));

    }
}
