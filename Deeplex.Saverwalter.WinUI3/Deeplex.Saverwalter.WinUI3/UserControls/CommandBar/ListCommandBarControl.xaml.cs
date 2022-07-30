using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ListCommandBarControl : UserControl
    {
        public ListCommandBarControl()
        {
            InitializeComponent();
        }

        public IListViewModel ViewModel
        {
            get { return (IListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(IListViewModel),
            typeof(ListCommandBarControl),
            new PropertyMetadata(null));
    }
}
