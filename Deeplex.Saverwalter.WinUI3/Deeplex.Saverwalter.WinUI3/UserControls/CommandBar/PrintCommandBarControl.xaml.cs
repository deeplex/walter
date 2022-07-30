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

        public IPrint ViewModel
        {
            get { return (IPrint)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(IPrint),
            typeof(PrintCommandBarControl),
            new PropertyMetadata(null));
    }
}
