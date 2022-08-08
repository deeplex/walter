using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ListCommandBarControl<T> : UserControl
    {
        public ListCommandBarControl()
        {
            InitializeComponent();
        }

        public IListViewModel<T> ViewModel
        {
            get { return (IListViewModel<T>)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                App.Window.CommandBar.Title = ViewModel.ToString();
            }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(IListViewModel<T>),
            typeof(ListCommandBarControl),
            new PropertyMetadata(null));
    }
}
