using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class DetailCommandBarControl<T> : UserControl
    {
        public DetailCommandBarControl()
        {
            InitializeComponent();
        }

        public IDetailViewModel<T> ViewModel
        {
            get { return (IDetailViewModel<T>)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                App.Window.CommandBar.Title = ViewModel.ToString();
            }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(IDetailViewModel<T>),
            typeof(DetailCommandBarControl),
            new PropertyMetadata(null));

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Delete.Execute(null); // Should be awaited?
            App.Window.AppFrame.GoBack();
        }
    }
}
