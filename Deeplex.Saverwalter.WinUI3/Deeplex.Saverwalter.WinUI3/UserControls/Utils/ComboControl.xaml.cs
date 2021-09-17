using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ComboControl : UserControl
    {
        public ComboControl()
        {
            InitializeComponent();
        }

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty
            = DependencyProperty.Register(
            "ItemsSource",
            typeof(object),
            typeof(ComboControl),
            new PropertyMetadata(null));

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty
            = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(ComboControl),
            new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
            "Header",
            typeof(string),
            typeof(ComboControl),
            new PropertyMetadata(""));

        public object PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderTextProperty
            = DependencyProperty.Register(
            "PlaceholderText",
            typeof(string),
            typeof(ComboControl),
            new PropertyMetadata(""));

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = (ComboBox)sender;
            if (cb.SelectedItem is string)
            {
                cb.SelectedItem = null;
            }
        }
    }
}
