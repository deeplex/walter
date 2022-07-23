using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class SingleItemCommandBarControl : UserControl
    {
        public SingleItemCommandBarControl()
        {
            InitializeComponent();
        }


        public ISingleItem ViewModel
        {
            get { return (ISingleItem)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(ISingleItem),
            typeof(SingleItemCommandBarControl),
            new PropertyMetadata(null));

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Delete.Execute(null); // Should be awaited?
            App.Window.AppFrame.GoBack();
        }
    }
}
