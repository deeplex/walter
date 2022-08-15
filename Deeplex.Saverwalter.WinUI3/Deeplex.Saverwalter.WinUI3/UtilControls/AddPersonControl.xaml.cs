using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class AddPersonControl : UserControl
    {
        public AddPersonControl()
        {
            InitializeComponent();
        }

        public IMemberListViewModel ViewModel
        {
            get { return (IMemberListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                "ViewModel",
                typeof(IMemberListViewModel),
                typeof(AddPersonControl),
                new PropertyMetadata(null));
    }
}
