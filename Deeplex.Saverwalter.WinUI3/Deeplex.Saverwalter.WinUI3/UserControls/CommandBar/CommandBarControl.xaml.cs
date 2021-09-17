using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class CommandBarControl : UserControl
    {
        public CommandBarControl()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(CommandBarControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(
            "MainContent",
            typeof(object),
            typeof(CommandBarControl),
            new PropertyMetadata(default(object)));

        public object MainContent
        {
            get { return GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        private void togglepane_Click(object sender, RoutedEventArgs e)
        {
            App.Window.SplitView.IsPaneOpen = !App.Window.SplitView.IsPaneOpen;
            togglepanesymbol.Symbol = App.Window.SplitView.IsPaneOpen ? Symbol.OpenPane : Symbol.ClosePane;
        }
    }
}
