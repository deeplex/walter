using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class PrintCommandBarControl : UserControl
    {
        public PrintCommandBarControl()
        {
            InitializeComponent();
        }

        public IPrintViewModel ViewModel
        {
            get { return (IPrintViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(IPrintViewModel),
            typeof(PrintCommandBarControl),
            new PropertyMetadata(null));
    }
}
