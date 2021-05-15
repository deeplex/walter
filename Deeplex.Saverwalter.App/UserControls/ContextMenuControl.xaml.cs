using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.ViewModels.Zähler;
using Deeplex.Saverwalter.App.Views;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class ContextMenuControl : UserControl
    {
        public ContextMenuControl()
        {
            InitializeComponent();
        }

        public object Item
        {
            get { return (object)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty
            = DependencyProperty.Register(
            "ItemProperty",
            typeof(object),
            typeof(ContextMenuControl),
            new PropertyMetadata(null));

        public bool Anhang
        {
            get { return (bool)GetValue(AnhangProperty); }
            set { SetValue(AnhangProperty, value); }
        }

        public static readonly DependencyProperty AnhangProperty
            = DependencyProperty.Register(
            "Anhang",
            typeof(bool),
            typeof(ContextMenuControl),
            new PropertyMetadata(false));

        public bool Navigation
        {
            get { return (bool)GetValue(NavigationProperty); }
            set { SetValue(NavigationProperty, value); }
        }

        public static readonly DependencyProperty NavigationProperty
            = DependencyProperty.Register(
            "Navigation",
            typeof(bool),
            typeof(ContextMenuControl),
            new PropertyMetadata(false));

        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            if (Item is WohnungListEntry w)
            {
                App.ViewModel.Navigate(typeof(WohnungDetailPage), w.Entity);
            }
            else if (Item is KontaktListEntry k)
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
            else if (Item is ZaehlerListEntry z)
            {
                App.ViewModel.Navigate(typeof(ZaehlerDetailPage), z.Entity);
            }
        }

        private void Anhaenge_Click(object sender, RoutedEventArgs e)
        {
            if (Item is WohnungListEntry w)
            {
                App.ViewModel.ListAnhang.Value = new AnhangListViewModel(w.Entity);
            }
            else if (Item is KontaktListEntry k)
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
            else if (Item is ZaehlerstandListEntry z)
            {
                App.ViewModel.ListAnhang.Value = new AnhangListViewModel(z.Entity);
            }
        }
    }
}
