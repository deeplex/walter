using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.ViewModels.Zähler;
using Deeplex.Saverwalter.App.Views;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
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
            typeof(object),
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

        public bool showNavigation =>
            SelectedItem is WohnungListEntry ||
            SelectedItem is KontaktListEntry ||
            SelectedItem is ZaehlerListEntry;

        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is WohnungListEntry w)
            {
                App.ViewModel.Navigate(typeof(WohnungDetailPage), w.Entity);
            }
            else if (SelectedItem is KontaktListEntry k)
            {
                var a = App.Walter.NatuerlichePersonen.SingleOrDefault(p => p.PersonId == k.Guid);
                if (a != null)
                {
                    App.ViewModel.Navigate(typeof(NatuerlichePersonDetailPage), a);
                }
                else
                {
                    var b = App.Walter.JuristischePersonen.SingleOrDefault(p => p.PersonId == k.Guid);
                    if (b != null)
                    {
                       App.ViewModel.Navigate(typeof(JuristischePersonenDetailPage), b);
                    }
                }
            }
            else if (SelectedItem is ZaehlerListEntry z)
            {
                App.ViewModel.Navigate(typeof(ZaehlerDetailPage), z.Entity);
            }
            ContextFlyout.Hide();
        }

        private void Anhaenge_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is WohnungListEntry w)
            {
                App.ViewModel.ListAnhang.Value = new AnhangListViewModel(w.Entity);
            }
            else if (SelectedItem is KontaktListEntry k)
            {
                var a = App.Walter.NatuerlichePersonen.SingleOrDefault(p => p.PersonId == k.Guid);
                if (a != null)
                {
                    App.ViewModel.ListAnhang.Value = new AnhangListViewModel(a);
                }
                else
                {
                    var b = App.Walter.JuristischePersonen.SingleOrDefault(p => p.PersonId == k.Guid);
                    if (b != null)
                    {
                        App.ViewModel.ListAnhang.Value = new AnhangListViewModel(b);
                    }
                }
            }
            else if (SelectedItem is ZaehlerstandListEntry z)
            {
                App.ViewModel.ListAnhang.Value = new AnhangListViewModel(z.Entity);
            }
            ContextFlyout.Hide();
        }
    }
}
